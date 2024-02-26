import { baseClassFile, expectedInterfaceFile, interfaceFile } from './test/test-class';
import { addMemberToDocument, getSignatureText } from './pull-to-interface-csharp';
import { SignatureLineResult, SignatureType } from './utils/csharp-util';
import * as vscodeMock from 'jest-mock-vscode';
import { MockTextEditor } from 'jest-mock-vscode/dist/vscode';
import { testFile } from './test/test-class';
import { Position, Selection, Uri } from 'vscode';

const TEST_ACCESSOR: string = 'public';

describe('Pull To Interface CSharp', () =>
{
  describe('addMemberToInterface', () =>
  {
    it('should return interface file with property included', () =>
    {

      const expected = expectedInterfaceFile;
      // Act
      const output = addMemberToDocument('IMyClass', new SignatureLineResult('int MyProperty { get; set; }', SignatureType.FullProperty, 1, 'public'), '\n', interfaceFile, true);

      expect(expected).toEqual(output);
    });

    it('should return base class file with protected method included', () =>
    {
      const signature = `
      protected Task<int> GetProtected<TNewType>(string name,
                                                  string address) where TNewType : TType
      {
          Console.WriteLine("protected");
          var coll = new List<string>();
      }`;
      // Act
      const output = addMemberToDocument('BaseClass', new SignatureLineResult(signature, SignatureType.FullProperty, 1, 'protected'), '\n', baseClassFile, false);

      expect(output).toContain('Console.WriteLine("protected");');
      expect(output).toContain('protected Task<int> GetProtected<TNewType>(string name,');
      expect(output).toContain('var coll = new List<string>();');
    });

  });
  describe('getMethodSignatureText', () =>
  {
    it('should return current method line when cursor is positioned in the member body', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(37, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signature).toEqual('Task<int> GetNewIdAsync<TNewType>(string name,string address,string city,string state) where TNewType : TType');
      expect(result?.signatureType).toEqual(SignatureType.Method);
    });

    it('should return null when when cursor is positioned in property', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(21, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signatureType).not.toEqual(SignatureType.Method);
    });

    it('should return null  when cursor is positioned in full lambda property', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(14, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signatureType).not.toEqual(SignatureType.Method);
    });

  });

  describe('getPropertySignatureText', () =>
  {
    it('should return SignatureType.Method when cursor is positioned in the method body', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(37, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signatureType).toEqual(SignatureType.Method);
    });

    it('should return current full property line when when cursor is positioned in property', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(21, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signature).toEqual('string FullPropertyAlt');
      expect(result?.signatureType).toEqual(SignatureType.FullProperty);
    });

    it('should return current auto property line when when cursor is positioned in property', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(10, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signature).toEqual('int MyProperty');
      expect(result?.signatureType).toEqual(SignatureType.FullProperty);
    });

    it('should return current lamda read only property line when when cursor is positioned in property', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(11, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signature).toEqual('int MyPropertyLamda');
      expect(result?.signatureType).toEqual(SignatureType.LambaProperty);
    });

    it('should return current full lambda property line  when cursor is positioned in full lambda property', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(14, 0)));

      const result = getSignatureText(editor, TEST_ACCESSOR);

      // Assert
      expect(result?.signature).toEqual('string FullProperty');
      expect(result?.signatureType).toEqual(SignatureType.FullProperty);
    });

  });

});


