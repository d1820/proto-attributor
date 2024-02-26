import { TextEditor } from 'vscode';

export interface IWindow
{
  activeTextEditor: TextEditor;
  showErrorMessage(message: string): Thenable<string>;
  showInformationMessage(message: string): Thenable<string>;
}
