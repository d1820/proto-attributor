import { IWindow } from '../interfaces/window.interface';
import { WorkspaceFolder } from 'vscode';

export const getWorkspaceFolder = (folders: WorkspaceFolder[] | undefined): string =>
{
  if (!folders)
  {
    return '';
  }

  const folder = folders[0];
  const uri = folder.uri;

  return uri.fsPath;
};

export const isWorkspaceLoaded = (workspaceRoot: string, window: IWindow) =>
{
  if (workspaceRoot === '')
  {
    window.showErrorMessage(
      'Please open a directory before trying to Pull To Interface.'
    );
    return false;
  }
  return true;
};

export const isTextEditorOpen = (window: IWindow) =>
{
  if (!window.activeTextEditor)
  {
    window.showErrorMessage('No open text editor. Please open a C# file.');
    return false;
  }
  return true;
};

export const isTextInEditor = (text: string | null, window: IWindow) =>
{
  if (!text)
  {
    window.showErrorMessage('No text found. Please open a C# file.');
    return false;
  }
  return true;
};
