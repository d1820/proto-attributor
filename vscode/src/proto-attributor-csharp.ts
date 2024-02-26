import { Range, Uri, WorkspaceEdit, workspace } from "vscode";
import { SignatureLineResult, getUsingStatementsFromText, replaceUsingStatementsFromText, getBeginningOfLineIndent, getEnumBody, SignatureType } from "./utils/csharp-util";


export async function applyEditsAsync (filePath: string, newFileContent: string): Promise<boolean>
{
  const edit = new WorkspaceEdit();
  const uri = Uri.file(filePath);

  // Replace a specific range of lines with new content
  edit.replace(uri, new Range(0, 0, Number.MAX_VALUE, 0), newFileContent);

  // Apply the edit
  return await workspace.applyEdit(edit);
};

export function getEditorDefaultIndent (): number
{
  const editorConfig = workspace.getConfiguration('editor');
  return editorConfig.get<number>('tabSize', 4); // Default to 4 spaces
};

export function hasAttribute (text: string, attr: string): boolean
{
  const regEx = new RegExp(`\\[${attr}.*?\\]`, 'gm');
  return regEx.test(text);
};

export function hasAttributeInLines (lines: string[] | null, attr: string): boolean
{
  if (!lines)
  {
    return false;
  }
  const regEx = new RegExp(`\\[${attr}.*?\\]`, 'gm');
  let match = false;
  lines.forEach(line =>
  {
    if (regEx.test(line))
    {
      match = true;
      return;
    }
  });
  return match;
};

export function getNextIndex (text: string, attr: string)
{
  const regEx = new RegExp(`\\[${attr}\\(.*?(\\d*)\\)\\]`, 'gm');
  const matches = [...text.matchAll(regEx)];
  let maxIndex = 0;
  var vals = matches.map(m => parseInt(m[1])) || [];
  vals.sort((a, b) => a - b);
  maxIndex = (vals.pop() || 0) + 1;
  return maxIndex;
};

export function addAttributeToDocument (
  eol: string,
  text: string,
  sig: SignatureLineResult,
  attr: string): string
{
  if (!sig.signature)
  {
    return text;
  }
  let indent = getBeginningOfLineIndent(sig.signature);
  const adjAttr = ''.padStart(indent, ' ') + attr;
  const adjSig = ''.padStart(sig.defaultLineIndent, ' ') + sig.signature;
  return text.replace(sig.signature, `${adjAttr}${eol}${adjSig}`);
};

export function addUsingsToDocument (
  eol: string,
  documentFileContent: string,
  usings: string[]): string
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

export function handleClassAttributes(fm: SignatureLineResult, eol: string, text: string, attrName: string, attr: string): string
{
  if (!hasAttributeInLines(fm.leadingTrivia, attrName))
  {
    //add it
    text = addAttributeToDocument(eol, text, fm, attr);
  }
  return text;
}

export function handleEnumAttributes(fm: SignatureLineResult, eol: string, text: string,
  attrName: string, attr: string,
  attrMemberName: string,
  attrMember: string): string
{
  if (!hasAttributeInLines(fm.leadingTrivia, attrName))
  {
    //add it
    text = addAttributeToDocument(eol, text, fm, attr);
  }
  //need to read in the enum lines and then attribute them
  var enumLines = getEnumBody(text); // this will contain other attributes and comments
  enumLines.forEach(line =>
  {
    const enumLineParts = line.split(eol).filter(f => f.trim().length > 0);
    if (!hasAttributeInLines(enumLineParts, attrMemberName))
    {
      const lastLine = enumLineParts.pop();
      const sig = new SignatureLineResult(lastLine!, SignatureType.Enum, 0);
      text = addAttributeToDocument(eol, text, sig, attrMember);
    }
  });
  return text;
}

export function handlePropertyAttributes(fm: SignatureLineResult, eol: string, text: string,
  attrName: string, attr: string, ignoreAttrName: string, addedCallback: ()=> void): string
{
  if (!hasAttributeInLines(fm.leadingTrivia, attrName) && !hasAttributeInLines(fm.leadingTrivia, ignoreAttrName))
  {
    //add it
    text = addAttributeToDocument(eol, text, fm, attr);
    addedCallback();
  }
  return text;
}

