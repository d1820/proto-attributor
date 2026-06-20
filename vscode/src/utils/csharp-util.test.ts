import { Position, Selection, Uri } from 'vscode';
import { getClassName, getNamespace, SignatureType, getUsingStatements, replaceUsingStatementsFromText, getUsingStatementsFromText, getMemberName, getLineEndingFromDoc, getEnumBody, getBeginningOfLineIndent, cleanString, getAllPublicMembers, getLeadingTrivia, SignatureLineResult } from './csharp-util';

import * as vscodeMock from 'jest-mock-vscode';
import { MockTextEditor } from 'jest-mock-vscode/dist/vscode';
import { testAddLinesBetweenMembers, testAddLinesBetweenMembersExpected, testFile, testTextWithProperNewLines, testTextWithProperNewLinesExpected} from '../test/test-class';
import { protoEnum } from '../test/proto-test-class';
import exp from 'constants';


describe('CSharp Util', () =>
{

  describe('getNamespace', () =>
  {
    it('should return the namespace', () =>
    {
      // Arrange
      const windowMock = {
        showErrorMessage: jest.fn()
      };
      const text = `namespace Test
    {
    }`;

      // Act
      const name = getNamespace(text, windowMock as any);

      // Assert
      expect(name).toBe('Test');
      expect(windowMock.showErrorMessage).not.toHaveBeenCalled();
    });

    it('should return null and an error message if the namespace in the file is not found', () =>
    {
      // Arrange
      const windowMock = {
        showErrorMessage: jest.fn()
      };
      const text = 'foo bar';

      // Act
      const name = getNamespace(text, windowMock as any);

      // Assert
      expect(name).toBe(null);
      expect(windowMock.showErrorMessage).toHaveBeenCalled();
    });
  });

  describe('getClassName', () =>
  {

    it('should return the name of the class in the file', () =>
    {
      // Arrange
      const windowMock = {
        showErrorMessage: jest.fn()
      };
      const text = `namespace Test
    {
        public class TestModel
        {
            public string StringTest { get; set; }
        }
    }`;

      // Act
      const name = getClassName(text, windowMock as any);

      // Assert
      expect(name).toBe('TestModel');
      expect(windowMock.showErrorMessage).not.toHaveBeenCalled();
    });

    it('should return the name of the class in the file in an abstract class', () =>
    {
      // Arrange
      const windowMock = {
        showErrorMessage: jest.fn()
      };
      const text = `namespace Test
    {
        public abstract class TestModel
        {
            public string StringTest { get; set; }
        }
    }`;

      // Act
      const name = getClassName(text, windowMock as any);

      // Assert
      expect(name).toBe('TestModel');
      expect(windowMock.showErrorMessage).not.toHaveBeenCalled();
    });

    it('should return null and an error message if the model name in the file is not found', () =>
    {
      // Arrange
      const windowMock = {
        showErrorMessage: jest.fn()
      };
      const text = 'foo bar';

      // Act
      const name = getClassName(text, windowMock as any);

      // Assert
      expect(name).toBe(null);
      expect(windowMock.showErrorMessage).toHaveBeenCalled();
    });
  });

  describe('getMemberName', () =>
  {

    it('should return the name of the property member in the file when property has generic', () =>
    {

      const text = 'public MyClass<string, int> StringTest { get; set; }';

      // Act
      const name = getMemberName(text);

      // Assert
      expect(name).toBe('StringTest');
    });

    it('should return the name of the property member in the file when property has tuple', () =>
    {

      const text = 'public (street: string, name: string) StringTest { get; set; }';

      // Act
      const name = getMemberName(text);

      // Assert
      expect(name).toBe('StringTest');
    });

    it('should return the name of the method member in the file', () =>
    {

      const text = 'public Task<int> StringTest()';

      // Act
      const name = getMemberName(text);

      // Assert
      expect(name).toBe('StringTest');
    });

    it('should return the name of the method member in the file when no accessor is given', () =>
    {

      const text = 'Task<int> StringTest()';

      // Act
      const name = getMemberName(text);

      // Assert
      expect(name).toBe('StringTest');
    });

    it('should return undefined if the member name in the file is not found', () =>
    {
      // Arrange
      const text = 'foo bar';

      // Act
      const name = getMemberName(text);

      // Assert
      expect(name).toBe(undefined);
    });
  });

  describe('getLineEnding', () =>
  {
    it('should return LF for LF document', () =>
    {
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const result = getLineEndingFromDoc(doc);
      expect(result).toEqual('\n');
    });

  });

  describe('getUsingStatements', () =>
  {
    it('should return array of using statements', () =>
    {
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const editor = new MockTextEditor(jest, doc, undefined, new Selection(new Position(1, 0), new Position(1, 0)));
      const eol = getLineEndingFromDoc(doc);
      const result = getUsingStatements(editor, eol);
      expect(result).toHaveLength(4);
      expect(result[0]).toEqual('using System;');
    });
  });

  describe('replaceUsingStatements', () =>
  {
    it('should return array of using statements', () =>
    {
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const eol = getLineEndingFromDoc(doc);
      const result = replaceUsingStatementsFromText(doc.getText(), ['using NoMatch;'], eol);
      expect(result).toContain('using NoMatch;');
      const items = getUsingStatementsFromText(result, eol);
      expect(items).toHaveLength(1);
    });
  });

  describe('getEnumBody', () =>
  {
    it('should return array of enum items', () =>
    {
      const bodyItems = getEnumBody(protoEnum);
      expect(bodyItems).toHaveLength(3);
    });

    it('should return split each line correctly', () =>
    {
      const bodyItems = getEnumBody(protoEnum);
      const parts = bodyItems[0].split('\n');
      expect(parts).toHaveLength(3);
      expect(parts[2]).toBe('      One');
    });
  });

  describe('getUsingStatementsFromText', () =>
  {
    it('should extract using statements from text', () =>
    {
      const text = 'using System;\nusing ProtoBuf;\n\npublic class Foo {}';
      const result = getUsingStatementsFromText(text, '\n');
      expect(result).toHaveLength(2);
      expect(result[0]).toBe('using System;');
      expect(result[1]).toBe('using ProtoBuf;');
    });

    it('should return empty array when no usings', () =>
    {
      const result = getUsingStatementsFromText('public class Foo {}', '\n');
      expect(result).toHaveLength(0);
    });
  });

  describe('getBeginningOfLineIndent', () =>
  {
    it('should return 0 for no indentation', () =>
    {
      expect(getBeginningOfLineIndent('public class Foo')).toBe(0);
    });

    it('should return 4 for 4-space indent', () =>
    {
      expect(getBeginningOfLineIndent('    public int Prop')).toBe(4);
    });

    it('should return 2 for 2-space indent', () =>
    {
      expect(getBeginningOfLineIndent('  public int Prop')).toBe(2);
    });

    it('should return 0 for empty string', () =>
    {
      expect(getBeginningOfLineIndent('')).toBe(0);
    });
  });

  describe('cleanString', () =>
  {
    it('should return null when input is null', () =>
    {
      expect(cleanString(null)).toBeNull();
    });

    it('should trim leading and trailing whitespace', () =>
    {
      expect(cleanString('  foo  ')).toBe('foo');
    });

    it('should collapse runs of 2+ whitespace', () =>
    {
      expect(cleanString('foo  bar')).toBe('foobar');
    });
  });

  describe('getLeadingTrivia - bug: crashes when member is on line 0', () =>
  {
    it('should not throw when lineMatchStartsOn is 0', () =>
    {
      // Bug: preSignatureStartingLine = 0 - 1 = -1, document.lineAt(-1) throws RangeError
      const text = 'public class Foo {}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const sig = new SignatureLineResult('public class Foo {}', SignatureType.Class, 0);
      expect(() => getLeadingTrivia(doc, sig)).not.toThrow();
    });
  });

  describe('getEnumBody - bug: crashes when text has no enum', () =>
  {
    it('should return empty array when text contains no public enum', () =>
    {
      // Bug: body! asserts non-null but regex.exec returns null when no match,
      // causing TypeError: Cannot read properties of null (reading 'length')
      expect(() => getEnumBody('public class Foo {}')).not.toThrow();
      expect(getEnumBody('public class Foo {}')).toHaveLength(0);
    });
  });

  describe('getLeadingTrivia', () =>
  {
    it('should collect attribute lines above the signature', () =>
    {
      const text = 'using System;\n\n[ProtoContract]\npublic class Foo {\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const sig = new SignatureLineResult('public class Foo {', SignatureType.Class, 3);
      getLeadingTrivia(doc, sig);
      expect(sig.leadingTrivia).toContain('[ProtoContract]');
    });

    it('should collect xml doc comment lines above the signature', () =>
    {
      const text = 'using System;\n\n///summary\npublic int Prop { get; set; }';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const sig = new SignatureLineResult('public int Prop { get; set; }', SignatureType.FullProperty, 3);
      getLeadingTrivia(doc, sig);
      expect(sig.leadingTrivia).toContain('///summary');
    });

    it('should stop at closing brace', () =>
    {
      const text = 'namespace Foo {\n}\npublic class Bar {\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const sig = new SignatureLineResult('public class Bar {', SignatureType.Class, 2);
      getLeadingTrivia(doc, sig);
      expect(sig.leadingTrivia).toHaveLength(0);
    });

    it('should stop at semicolon line (lambda property above)', () =>
    {
      const text = 'using System;\n\npublic class Foo {\n    public int A { get; set; }\n\n    public int B { get; set; }\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const sig = new SignatureLineResult('public int B { get; set; }', SignatureType.FullProperty, 5);
      getLeadingTrivia(doc, sig);
      expect(sig.leadingTrivia).toHaveLength(0);
    });
  });

  describe('getAllPublicMembers', () =>
  {
    it('should classify a class signature', () =>
    {
      const text = 'using System;\n\npublic class Foo {\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const members = getAllPublicMembers(text, doc);
      expect(members.some(m => m.signatureType === SignatureType.Class)).toBe(true);
    });

    it('should classify an enum signature', () =>
    {
      const text = 'using System;\n\npublic enum Status {\n    Active\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const members = getAllPublicMembers(text, doc);
      expect(members.some(m => m.signatureType === SignatureType.Enum)).toBe(true);
    });

    it('should classify a method signature', () =>
    {
      const text = 'using System;\n\npublic class Foo {\n    public void Bar() { }\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const members = getAllPublicMembers(text, doc);
      expect(members.some(m => m.signatureType === SignatureType.Method)).toBe(true);
    });

    it('should classify a lambda property signature', () =>
    {
      const text = 'using System;\n\npublic class Foo {\n    public string Greeting => "Hello";\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const members = getAllPublicMembers(text, doc);
      expect(members.some(m => m.signatureType === SignatureType.LambaProperty)).toBe(true);
    });

    it('should classify a full property signature', () =>
    {
      const text = 'using System;\n\npublic class Foo {\n    public string Name { get; set; }\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const members = getAllPublicMembers(text, doc);
      expect(members.some(m => m.signatureType === SignatureType.FullProperty)).toBe(true);
    });

    it('should return empty array when no public members', () =>
    {
      const text = 'using System;\n\nnamespace Foo {}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const members = getAllPublicMembers(text, doc);
      expect(members).toHaveLength(0);
    });

    it('should attach leading trivia attributes to signatures', () =>
    {
      const text = 'using System;\n\n[ProtoContract]\npublic class Foo {\n}';
      const doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), text, 'csharp');
      const members = getAllPublicMembers(text, doc);
      const cls = members.find(m => m.signatureType === SignatureType.Class);
      expect(cls?.leadingTrivia).toContain('[ProtoContract]');
    });
  });
});


