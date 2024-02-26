// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import { getWorkspaceFolder } from './utils/workspace-util';
import * as csharp from './proto-attributor-csharp';
import { IWindow } from './interfaces/window.interface';
import { SignatureLineResult, SignatureType, addLineBetweenMembers, checkIfAlreadyPulledToInterface, formatTextWithProperNewLines, getLineEnding, getMemberBodyByBrackets, getMemberBodyBySemiColon, getMemberName, getUsingStatements } from './utils/csharp-util';


const addCommand = 'protoattributor.add';
const reorderCommand = 'protoattributor.reorder';
const removeCommand = 'protoattributor.reove';
//let allCommands: any = null;
// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export async function activate(context: vscode.ExtensionContext)
{
  var wsf = vscode.workspace.workspaceFolders;
  const workspaceRoot: string = getWorkspaceFolder(wsf as vscode.WorkspaceFolder[]);

  let disposable = vscode.commands.registerTextEditorCommand(addCommand, async (editor: vscode.TextEditor) =>
  {
    if (editor && editor.document.languageId === 'csharp')
    {
      // let subCommands = await csharp.getSubCommandsAsync(workspaceRoot, vscode.window as IWindow);
      // const signatureResult = csharp.getSignatureToPull(editor, '(public|protected)');
      // if (signatureResult?.accessor === 'protected')
      // {
      //   //filter out Interfaces
      //   subCommands = subCommands.filter(f => !f.startsWith('I'));
      // }
      // buildSubCommands(subCommands, context);
    }
    else
    {
      vscode.window.showErrorMessage(`Unsupported pull. Language ${editor.document.languageId} not supported.`);
    }
  });

  context.subscriptions.push(disposable);
}

const isSubcommandRegisteredAsync = async (subcommand: string): Promise<boolean> =>
{
  const allCommands = await vscode.commands.getCommands(true);
  return allCommands.includes(subcommand);
};

// const buildSubCommands = async (subcommands: string[], context: vscode.ExtensionContext) =>
// {
//   // Register each subcommand
//   subcommands.forEach(async subcommand =>
//   {
//     const subCommandName = `${extensionName}.${subcommand}`;
//     const isRegistered = await isSubcommandRegisteredAsync(subCommandName);
//     if (!isRegistered)
//     {
//       const disposable = vscode.commands.registerTextEditorCommand(subCommandName, async (editor: vscode.TextEditor) =>
//       {
//         const signatureResult = csharp.getSignatureToPull(editor, '(public|protected)');
//         let methodBodySignature: SignatureLineResult | null = null;

//         //check if eligible for pull
//         if (!signatureResult?.signature || signatureResult.signatureType === SignatureType.Unknown)
//         {
//           vscode.window.showErrorMessage(`Unsupported pull. Unable to determine what to pull. 'public' properties and 'public' or 'protected' methods are only supported. Please copy manually`);
//           return;
//         }

//         //read file contents
//         const files = await vscode.workspace.findFiles(`**/${subcommand}.cs`, '**/node_modules/**');
//         if (files.length > 1)
//         {
//           vscode.window.showErrorMessage(`More then one file found matching ${subcommand}. Please copy manually`);
//           return;
//         }
//         if (files.length === 0)
//         {
//           vscode.window.showErrorMessage(`No files found matching ${subcommand}. Please copy manually`);
//           return;
//         }
//         const eol = getLineEnding(editor);
//         const selectedFileDocument = await vscode.workspace.openTextDocument(files[0].path);
//         let selectedFileDocumentContent = selectedFileDocument.getText();
//         if (!selectedFileDocumentContent)
//         {
//           vscode.window.showErrorMessage(`Unable to parse file ${subcommand}. Please copy manually`);
//           return;
//         }

//         if (checkIfAlreadyPulledToInterface(selectedFileDocumentContent, signatureResult, eol))
//         {
//           vscode.window.showWarningMessage(`Member already in ${subcommand}. Skipping pull`);
//           return;
//         }
//         else
//         {
//           if (!subcommand.startsWith("I")) //base class
//           {
//             let currentLine = editor.document.lineAt(signatureResult.lineMatchStartsOn).text;
//             if (currentLine.indexOf("=>") > -1)
//             {
//               const body = getMemberBodyBySemiColon(editor, signatureResult);
//               methodBodySignature = new SignatureLineResult(body, signatureResult.signatureType, signatureResult.lineMatchStartsOn, signatureResult.accessor);
//               methodBodySignature.preSignatureContent = signatureResult.preSignatureContent;
//               selectedFileDocumentContent = csharp.addMemberToDocument(subcommand, methodBodySignature, eol, selectedFileDocumentContent, false);

//             }
//             else
//             {
//               const body = getMemberBodyByBrackets(editor, signatureResult);
//               methodBodySignature = new SignatureLineResult(body, signatureResult.signatureType, signatureResult.lineMatchStartsOn, signatureResult.accessor);
//               methodBodySignature.preSignatureContent = signatureResult.preSignatureContent;
//               selectedFileDocumentContent = csharp.addMemberToDocument(subcommand, methodBodySignature, eol, selectedFileDocumentContent, false);
//             }
//           }
//           else
//           {
//             if (signatureResult.accessor === 'protected')
//             {
//               vscode.window.showErrorMessage(`Unsupported pull. Protected members can not be pulled to an inteface. Please copy method manually`);
//               return;
//             }
//             selectedFileDocumentContent = csharp.addMemberToDocument(subcommand, signatureResult, eol, selectedFileDocumentContent, true);
//           }

//           if (selectedFileDocumentContent)
//           {
//             const currentDocumentUsings = getUsingStatements(editor, eol);
//             selectedFileDocumentContent = csharp.addUsingsToDocument(eol, selectedFileDocumentContent, currentDocumentUsings);
//             selectedFileDocumentContent = formatTextWithProperNewLines(selectedFileDocumentContent, eol);
//             selectedFileDocumentContent = addLineBetweenMembers(selectedFileDocumentContent,eol);
//             const success = await csharp.applyEditsAsync(files[0].path, selectedFileDocumentContent);
//             if (!success)
//             {
//               vscode.window.showErrorMessage(`Unable to update ${subcommand}. Please copy manually`);
//               return;
//             }

//             const wasSaved = await selectedFileDocument.save();
//             if (!wasSaved)
//             {
//               vscode.window.showWarningMessage(`Unable to save updates to ${subcommand}. Please save file manually`);
//               return;
//             }
//             if (!subcommand.startsWith("I") && methodBodySignature?.signature)
//             {
//               //remove if from current file
//               const activeFileUrl = editor.document.uri.path;
//               const currentFileDocument = await vscode.workspace.openTextDocument(activeFileUrl);
//               const success = await removeFromCurrentEditorAsync(currentFileDocument, methodBodySignature, eol, activeFileUrl);
//               if (!success)
//               {
//                 vscode.window.showErrorMessage(`Unable to remove ${methodBodySignature.signatureType}. Please remove manually`);
//                 return;
//               }
//               const wasSaved = await currentFileDocument.save();
//               if (!wasSaved)
//               {
//                 vscode.window.showErrorMessage(`Unable to remove ${methodBodySignature.signatureType}. Please remove manually`);
//                 return;
//               }
//             }
//             const memberName = getMemberName(signatureResult!.signature);
//             vscode.window.showInformationMessage(`${memberName} pulled to ${subcommand}`);
//           }
//           else
//           {
//             vscode.window.showErrorMessage(`Unable to parse file ${subcommand}. Please copy manually`);
//           }
//         }
//       });

//       context.subscriptions.push(disposable);
//     }
//   });
//   // Show a quick pick to execute subcommands
//   const chosenSubcommand = await vscode.window.showQuickPick(subcommands);

//   // Execute the chosen subcommand
//   if (chosenSubcommand)
//   {
//     vscode.commands.executeCommand(`${extensionName}.${chosenSubcommand}`);
//   }
// };

async function removeFromCurrentEditorAsync(currentFileDocument: vscode.TextDocument, methodBodySignature: SignatureLineResult, eol: string, activeFileUrl: string)
{
  let currentFileDocumentContent = currentFileDocument.getText();
  currentFileDocumentContent = currentFileDocumentContent.replace(methodBodySignature.signature + eol, '');
  currentFileDocumentContent = formatTextWithProperNewLines(currentFileDocumentContent, eol);
  currentFileDocumentContent = addLineBetweenMembers(currentFileDocumentContent,eol);
  const success = await csharp.applyEditsAsync(activeFileUrl, currentFileDocumentContent);
  return success;
}

// This method is called when your extension is deactivated
export function deactivate()
{ }
