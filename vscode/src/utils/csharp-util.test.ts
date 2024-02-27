import { Position, Selection, Uri } from 'vscode';
import { getClassName,  getNamespace,  SignatureType,  getUsingStatements, replaceUsingStatementsFromText, getUsingStatementsFromText, getMemberName,  getLineEndingFromDoc,  getEnumBody } from './csharp-util';

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
    it('should CRLF as line ending', () =>
    {
      // Act
      var doc = vscodeMock.createTextDocument(Uri.parse('C:\temp\test.cs'), testFile, 'csharp');
      const result = getLineEndingFromDoc(doc);

      // Assert
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
});


