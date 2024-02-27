import
  {
    getWorkspaceFolder,
    isTextEditorOpen,
    isTextInEditor,
    isWorkspaceLoaded
  } from './workspace-util';

describe('Workspace Util', () =>
{
  afterEach(() =>
  {
    jest.clearAllMocks();
  });

  it('should return empty string if workspace folders are empty', () =>
  {
    // Arrange
    const folders = undefined;

    // Act
    const wsRoot = getWorkspaceFolder(folders);

    // Assert
    expect(wsRoot).toBe('');
  });

  it('should return workspace root path', () =>
  {
    // Arrange
    const folders: any[] = [{ uri: { fsPath: 'path/to/unit/test/' } }];

    // Act
    const wsRoot = getWorkspaceFolder(folders);

    // Assert
    expect(wsRoot).toBe('path/to/unit/test/');
  });

  it('should return false if a workspace is not loaded and show an error message', () =>
  {
    // Arrange
    const windowMock = {
      showErrorMessage: jest.fn()
    };
    const workspaceRoot = '';

    // Act
    const loaded = isWorkspaceLoaded(workspaceRoot, windowMock as any);

    // Assert
    expect(loaded).toBeFalsy();
    expect(windowMock.showErrorMessage).toHaveBeenCalled();
  });

  it('should return true if a workspace is loaded and not show an error message', () =>
  {
    // Arrange
    const windowMock = {
      showErrorMessage: jest.fn()
    };
    const workspaceRoot = 'foo';

    // Act
    const loaded = isWorkspaceLoaded(workspaceRoot, windowMock as any);

    // Assert
    expect(loaded).toBeTruthy();
    expect(windowMock.showErrorMessage).not.toHaveBeenCalled();
  });

  it('should return false if a text editor is not open and show an error message', () =>
  {
    // Arrange
    const windowMock = {
      activeTextEditor: false,
      showErrorMessage: jest.fn()
    };

    // Act
    const open = isTextEditorOpen(windowMock as any);

    // Assert
    expect(open).toBeFalsy();
    expect(windowMock.showErrorMessage).toHaveBeenCalled();
  });

  it('should return true if a text editor is open and not show an error message', () =>
  {
    // Arrange
    const windowMock = {
      activeTextEditor: true,
      showErrorMessage: jest.fn()
    };

    // Act
    const open = isTextEditorOpen(windowMock as any);

    // Assert
    expect(open).toBeTruthy();
    expect(windowMock.showErrorMessage).not.toHaveBeenCalled();
  });

  it('should return false if text is not in editor and show an error message', () =>
  {
    // Arrange
    const windowMock = {
      showErrorMessage: jest.fn()
    };
    const text = '';

    // Act
    const textInEditor = isTextInEditor(text, windowMock as any);

    // Assert
    expect(textInEditor).toBeFalsy();
    expect(windowMock.showErrorMessage).toHaveBeenCalled();
  });

  it('should return true if text is in editor and not show an error message', () =>
  {
    // Arrange
    const windowMock = {
      showErrorMessage: jest.fn()
    };
    const text = 'foo';

    // Act
    const textInEditor = isTextInEditor(text, windowMock as any);

    // Assert
    expect(textInEditor).toBeTruthy();
    expect(windowMock.showErrorMessage).not.toHaveBeenCalled();
  });
});
