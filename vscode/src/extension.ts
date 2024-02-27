// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import { getWorkspaceFolder, isTextEditorOpen, isWorkspaceLoaded } from './utils/workspace-util';
import { IWindow } from './interfaces/window.interface';
import { SignatureType, getAllPublicMembers, getLineEndingFromDoc } from './utils/csharp-util';
import { handleClassAttributes, addUsingsToDocument, applyEditsAsync, getNextIndex, handleEnumAttributes, handlePropertyAttributes, handlePropertyAttributeReorder, removeUsingsFromDocument, removeClassAttributeFromDocument, removePropertyAttributeFromDocument } from './proto-attributor-csharp';
import { Data, Proto } from './utils/constants';


const addCommand = 'protoattributor.add';
const reorderCommand = 'protoattributor.reorder';
const removeCommand = 'protoattributor.remove';
const currentWindow = vscode.window as IWindow;
const protoSubCommandText = 'Proto Attributes';
const dataContractSubCommandText = 'Data Contact Attributes';
const subcommandOptions = [protoSubCommandText, dataContractSubCommandText];
//let allCommands: any = null;
// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export async function activate(context: vscode.ExtensionContext)
{
  var wsf = vscode.workspace.workspaceFolders;
  const workspaceRoot: string = getWorkspaceFolder(wsf as vscode.WorkspaceFolder[]);

  if (!isWorkspaceLoaded(workspaceRoot, currentWindow))
  {
    return;
  };
  if (!isTextEditorOpen(currentWindow))
  {
    return;
  };

  context.subscriptions.push(registerAddCommands(context));
  context.subscriptions.push(registerReorderCommands(context));
  context.subscriptions.push(registerRemoveCommands(context));
}

function registerAddCommands(context: vscode.ExtensionContext)
{
  return vscode.commands.registerTextEditorCommand(addCommand, async (editor: vscode.TextEditor) =>
  {
    if (editor && editor.document.languageId === 'csharp')
    {
      const chosenSubcommand = await vscode.window.showQuickPick(subcommandOptions);
      if (chosenSubcommand)
      {
        let text = editor.document.getText();
        const eol = getLineEndingFromDoc(editor.document);
        if (chosenSubcommand === protoSubCommandText)
        {
          text = addUsingsToDocument(eol, text, [`using ${Proto.USING_STATEMENT};`]);
          const fileMembers = getAllPublicMembers(text, editor.document);
          let nextIndex = getNextIndex(text, Proto.PROPERTY_ATTRIBUTE_NAME);
          fileMembers.forEach(fm =>
          {
            switch (fm.signatureType)
            {
              case SignatureType.Class:
                {
                  text = handleClassAttributes(fm, eol, text, Proto.CLASS_ATTRIBUTE_NAME, `[${Proto.CLASS_ATTRIBUTE_NAME}]`);
                  break;
                }
              case SignatureType.Enum:
                {
                  text = handleEnumAttributes(fm, eol, text,
                    Proto.ENUM_ATTRIBUTE_NAME,
                    `[${Proto.ENUM_ATTRIBUTE_NAME}]`,
                    Proto.ENUM_MEMBER_NAME,
                    `[${Proto.ENUM_MEMBER_NAME}]`);
                  break;
                }
              case SignatureType.FullProperty:
              case SignatureType.LambaProperty:
                {
                  text = handlePropertyAttributes(fm, eol, text,
                    Proto.PROPERTY_ATTRIBUTE_NAME,
                    `[${Proto.PROPERTY_ATTRIBUTE_NAME}(${nextIndex})]`,
                    Proto.PROPERTY_IGNORE_ATTRIBUTE_NAME,
                    () =>
                    {
                      nextIndex++;
                    });
                  break;
                }
              case SignatureType.Method:
              case SignatureType.Unknown:
                {
                  break;
                }
            }
          });

          const success = await applyEditsAsync(editor.document.fileName, text);
          if (!success)
          {
            vscode.window.showErrorMessage(`Unable to add proto attributes. Please add manually`);
          }
          return;
        }
        text = addUsingsToDocument(eol, text, [`using ${Data.USING_STATEMENT};`]);
        const fileMembers = getAllPublicMembers(text, editor.document);
        let nextIndex = getNextIndex(text, Data.PROPERTY_ATTRIBUTE_NAME);
        fileMembers.forEach(fm =>
        {
          switch (fm.signatureType)
          {
            case SignatureType.Class:
              {
                text = handleClassAttributes(fm, eol, text, Data.CLASS_ATTRIBUTE_NAME, `[${Data.CLASS_ATTRIBUTE_NAME}]`);
                break;
              }
            case SignatureType.Enum:
              {
                text = handleEnumAttributes(fm, eol, text,
                  Data.ENUM_ATTRIBUTE_NAME,
                  `[${Data.ENUM_ATTRIBUTE_NAME}]`,
                  Data.ENUM_MEMBER_NAME,
                  `[${Data.ENUM_MEMBER_NAME}]`);
                break;
              }
            case SignatureType.FullProperty:
            case SignatureType.LambaProperty:
              {
                text = handlePropertyAttributes(fm, eol, text,
                  Data.PROPERTY_ATTRIBUTE_NAME,
                  `[${Data.PROPERTY_ATTRIBUTE_NAME}(Order = ${nextIndex})]`,
                  Data.PROPERTY_IGNORE_ATTRIBUTE_NAME,
                  () =>
                  {
                    nextIndex++;
                  });
                break;
              }
            case SignatureType.Method:
            case SignatureType.Unknown:
              {
                break;
              }
          }
        });
        const success = await applyEditsAsync(editor.document.fileName, text);
        if (!success)
        {
          vscode.window.showErrorMessage(`Unable to add data contract attributes. Please add manually`);
        }
      }
    }
    else
    {
      vscode.window.showErrorMessage(`Unsupported action. Language ${editor.document.languageId} not supported.`);
    }
  });
}

function registerReorderCommands(context: vscode.ExtensionContext)
{
  return vscode.commands.registerTextEditorCommand(reorderCommand, async (editor: vscode.TextEditor) =>
  {
    if (editor && editor.document.languageId === 'csharp')
    {
      const chosenSubcommand = await vscode.window.showQuickPick(subcommandOptions);
      if (chosenSubcommand)
      {
        let text = editor.document.getText();
        const eol = getLineEndingFromDoc(editor.document);
        if (chosenSubcommand === protoSubCommandText)
        {
          text = addUsingsToDocument(eol, text, [`using ${Proto.USING_STATEMENT};`]);
          const fileMembers = getAllPublicMembers(text, editor.document);
          fileMembers.forEach(fm =>
          {
            switch (fm.signatureType)
            {
              case SignatureType.Class:
                {
                  text = handleClassAttributes(fm, eol, text, Proto.CLASS_ATTRIBUTE_NAME, `[${Proto.CLASS_ATTRIBUTE_NAME}]`);
                  break;
                }
              case SignatureType.FullProperty:
              case SignatureType.LambaProperty:
                {
                  text = handlePropertyAttributeReorder(text, Proto.PROPERTY_ATTRIBUTE_NAME);
                  break;
                }
              case SignatureType.Enum:
              case SignatureType.Method:
              case SignatureType.Unknown:
                {
                  break;
                }
            }
          });

          const success = await applyEditsAsync(editor.document.fileName, text);
          if (!success)
          {
            vscode.window.showErrorMessage(`Unable to reorder proto attributes. Please reorder manually`);
          }
          return;
        }

        text = addUsingsToDocument(eol, text, [`using ${Data.USING_STATEMENT};`]);
        const fileMembers = getAllPublicMembers(text, editor.document);
        fileMembers.forEach(fm =>
        {
          switch (fm.signatureType)
          {
            case SignatureType.Class:
              {
                text = handleClassAttributes(fm, eol, text, Data.CLASS_ATTRIBUTE_NAME, `[${Data.CLASS_ATTRIBUTE_NAME}]`);
                break;
              }
            case SignatureType.FullProperty:
            case SignatureType.LambaProperty:
              {
                text = handlePropertyAttributeReorder(text, Data.PROPERTY_ATTRIBUTE_NAME);
                break;
              }
            case SignatureType.Enum:
            case SignatureType.Method:
            case SignatureType.Unknown:
              {
                break;
              }
          }
        });

        const success = await applyEditsAsync(editor.document.fileName, text);
        if (!success)
        {
          vscode.window.showErrorMessage(`Unable to reorder data contract attributes. Please reorder manually`);
        }
      }
    }

    else
    {
      vscode.window.showErrorMessage(`Unsupported action. Language ${editor.document.languageId} not supported.`);
    }
  });
}

function registerRemoveCommands(context: vscode.ExtensionContext)
{
  return vscode.commands.registerTextEditorCommand(removeCommand, async (editor: vscode.TextEditor) =>
  {
    if (editor && editor.document.languageId === 'csharp')
    {
      const chosenSubcommand = await vscode.window.showQuickPick(subcommandOptions);
      if (chosenSubcommand)
      {
        let text = editor.document.getText();
        const eol = getLineEndingFromDoc(editor.document);
        if (chosenSubcommand === protoSubCommandText)
        {
          text = removeUsingsFromDocument(eol, text, [`using ${Proto.USING_STATEMENT};`]);
          text = removeClassAttributeFromDocument(eol, text, Proto.CLASS_ATTRIBUTE_NAME);
          text = removePropertyAttributeFromDocument(eol, text, Proto.PROPERTY_ATTRIBUTE_NAME);
          text = removePropertyAttributeFromDocument(eol, text, Proto.PROPERTY_IGNORE_ATTRIBUTE_NAME);
          const success = await applyEditsAsync(editor.document.fileName, text);
          if (!success)
          {
            vscode.window.showErrorMessage(`Unable to remove proto attributes. Please remove manually`);
          }
          return;
        }
        text = removeUsingsFromDocument(eol, text, [`using ${Data.USING_STATEMENT};`]);
        text = removeClassAttributeFromDocument(eol, text, Data.CLASS_ATTRIBUTE_NAME);
        text = removePropertyAttributeFromDocument(eol, text, Data.PROPERTY_ATTRIBUTE_NAME);
        text = removePropertyAttributeFromDocument(eol, text, Data.PROPERTY_IGNORE_ATTRIBUTE_NAME);
        const success = await applyEditsAsync(editor.document.fileName, text);
        if (!success)
        {
          vscode.window.showErrorMessage(`Unable to remove proto attributes. Please remove manually`);
        }
        return;
      }
    }
    else
    {
      vscode.window.showErrorMessage(`Unsupported action. Language ${editor.document.languageId} not supported.`);
    }
  });
}

// This method is called when your extension is deactivated
export function deactivate()
{ }
