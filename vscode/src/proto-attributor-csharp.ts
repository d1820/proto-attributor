import { Range, TextEditor, Uri, WorkspaceEdit, workspace } from "vscode";
import { IWindow } from "./interfaces/window.interface";
import { getClassName, getInheritedNames, getNamespace, SignatureType, SignatureLineResult, getUsingStatementsFromText, replaceUsingStatementsFromText, getBeginningOfLineIndent, isValidAccessorLine, getFullSignatureOfLine, isTerminating, cleanString, cleanAccessor } from "./utils/csharp-util";
import { isTextEditorOpen, isTextInEditor, isWorkspaceLoaded } from "./utils/workspace-util";


export class InheritedMemberTracker
{
  names: string[] = [];
  constructor()
  {
  }
}
export const getSubCommandsAsync = async (workspaceRoot: string, window: IWindow): Promise<string[]> =>
{
  if (!isWorkspaceLoaded(workspaceRoot, window))
  {
    return [];
  };
  if (!isTextEditorOpen(window))
  {
    return [];
  };

  const editor = window.activeTextEditor;
  const text = editor.document.getText();
  const namespace = getNamespace(text, window);
  const className = getClassName(text, window);
  if (!isTextInEditor(text, window) || !namespace || !className)
  {
    return [];
  };

  let inheritedNames = getInheritedNames(text, true);

  const tracker = new InheritedMemberTracker();

  const promises = inheritedNames.map(fileName =>
    openFileAndReadInheritedNamesAsync(fileName, tracker)
  );
  const pr = await Promise.all(promises);
  pr.forEach(result =>
  {
    tracker.names.push(...result);
  });

  tracker.names.push(...inheritedNames);
  tracker.names =tracker.names.sort();
  return [...new Set(tracker.names)];
};

const openFileAndReadInheritedNamesAsync = async (fileName: string, tracker: InheritedMemberTracker): Promise<string[]> =>
{
  const files = await workspace.findFiles(`**/${fileName}.cs`, '**/node_modules/**');
  if (files.length > 1 || files.length === 0)
  {
    return [];
  }
  const document = await workspace.openTextDocument(files[0].path);
  const text = document.getText();
  const inheritedNames = getInheritedNames(text, true);
  if (inheritedNames.length > 0)
  {
    const promises = inheritedNames.map(fileName =>
      openFileAndReadInheritedNamesAsync(fileName, tracker)
    );
    const pr = await Promise.all(promises);
    pr.forEach(result =>
    {
      tracker.names.push(...result);
    });
  }
  return inheritedNames;
};

export const getSignatureToPull = (editor: TextEditor, accessor: string): SignatureLineResult | null =>
{
  const signature = getSignatureText(editor, accessor);

  if (signature?.signature)
  {
    if (signature.signatureType === SignatureType.Method)
    {
      return SignatureLineResult.createFromSignatureLineResult(`${signature.signature};`, signature);
    }
    if (signature.signatureType === SignatureType.FullProperty)
    {
      return SignatureLineResult.createFromSignatureLineResult(`${signature.signature} { get; set; }`, signature);
    }
    return SignatureLineResult.createFromSignatureLineResult(`${signature.signature} { get; }`, signature);
  }
  return null;
};

export const applyEditsAsync = async (filePath: string, newFileContent: string): Promise<boolean> =>
{
  const edit = new WorkspaceEdit();
  const uri = Uri.file(filePath);

  // Replace a specific range of lines with new content
  edit.replace(uri, new Range(0, 0, Number.MAX_VALUE, 0), newFileContent);

  // Apply the edit
  return await workspace.applyEdit(edit);
};

export const getEditorDefaultIndent = (): number =>
{
  const editorConfig = workspace.getConfiguration('editor');
  return editorConfig.get<number>('tabSize', 4); // Default to 4 spaces
};

export const addMemberToDocument = (subcommand: string,
  signatureResult: SignatureLineResult,
  eol: string,
  documentFileContent: string,
  isInterface: boolean): string =>
{

  if (!signatureResult.signature)
  {
    return documentFileContent;
  }
  let regEx;
  if (isInterface)
  {
    regEx = new RegExp(`(.*public\\s*interface\\s*${subcommand}.*[\\s]*{)`);
  }
  else
  {
    regEx = new RegExp(`(.*class\\s*${subcommand}.*[\\s]*{)`);
  }

  const documentMatchedMember = documentFileContent!.match(regEx);

  if (documentMatchedMember)
  {
    const originalText = documentMatchedMember[1]; //group from regex
    //get the indent count
    const beginningIndent = getBeginningOfLineIndent(originalText);
    let totalLength = signatureResult.signature.length;
    const indent = getEditorDefaultIndent();
    let newText;
    if (isInterface)
    {
      totalLength = totalLength + beginningIndent + indent;
      newText = `${originalText}${eol}${signatureResult.signature.padStart(totalLength, ' ')}${eol}`;
    }
    else
    {
      newText = `${originalText}${eol}${signatureResult.signature}${eol}`;
    }

    documentFileContent = documentFileContent!.replace(regEx, newText);
    return documentFileContent;
  }
  else
  {
    return documentFileContent;
  }
};

export const addUsingsToDocument = (
  eol: string,
  documentFileContent: string,
  usings: string[]): string =>
{

  if (!documentFileContent)
  {
    return documentFileContent;
  }
  //add the usings to file content
  const existingDocumentUsings = getUsingStatementsFromText(documentFileContent, eol);
  let combinedUsings = [...usings, ...existingDocumentUsings];
  combinedUsings = [...new Set(combinedUsings)]; //distinct
  documentFileContent = replaceUsingStatementsFromText(documentFileContent, combinedUsings, eol);
  return documentFileContent;
};

export const getSignatureText = (editor: TextEditor, accessor: string): SignatureLineResult | null =>
{
  let signatureResult: SignatureLineResult | null = null;
  if (editor)
  {
    // Get the position of the cursor
    const cursorPosition = editor.selection.active;
    let line = cursorPosition.line;
    let currentLine = editor.document.lineAt(line).text;
    const originalSelectedLine = currentLine;
    let accessMatch = isValidAccessorLine(currentLine, accessor);
    if (accessMatch)
    {
      signatureResult = getFullSignatureOfLine(accessor, editor, line);
    }
    else
    {
      while (!accessMatch && !isTerminating(currentLine))
      {
        if (line < 1)
        {
          break;
        }
        //we start reading up lines to get the starting line
        line = line - 1;
        currentLine = editor.document.lineAt(line).text;
        accessMatch = isValidAccessorLine(currentLine, accessor);
        if (accessMatch)
        {
          signatureResult = getFullSignatureOfLine(accessor, editor, line);
          break;
        }
      }
    }
    if (signatureResult?.signature)
    {
      signatureResult.signature = cleanString(signatureResult.signature);
      signatureResult.signature = cleanAccessor(accessor, signatureResult.signature!);
      signatureResult.originalSelectedLine = originalSelectedLine;
    }
  }
  return signatureResult;
  // if (checkForMethod)
  // {
  //   return isMethod(signatureResult?.signature) ? signatureResult : null;
  // }
  // return isMethod(signatureResult?.signature) ? null : signatureResult;
};

