import {
  getNextIndex,
  handlePropertyAttributeReorder,
  hasAttribute,
  hasAttributeInLines,
  getAllAttributes,
  addAttributeToDocument,
  addUsingsToDocument,
  removeUsingsFromDocument,
  removeClassAttributeFromDocument,
  removePropertyAttributeFromDocument,
  handleClassAttributes,
  handleEnumAttributes,
  handlePropertyAttributes,
} from './proto-attributor-csharp';
import { noProtoExisting, protoExisting, protoReorderExisting, protoReorderExistingExpected, protoEnum } from './test/proto-test-class';
import { dataContractExisting, noDataContractExisting } from './test/data-test-class';
import { Data, Proto } from './utils/constants';
import { SignatureLineResult, SignatureType } from './utils/csharp-util';

describe('Proto Attributor CSharp', () =>
{
  describe('getNextProtoIndex', () =>
  {
    it('should return next index from existing', () =>
    {
      const result = getNextIndex(protoExisting, Proto.PROPERTY_ATTRIBUTE_NAME);

      // Assert
      expect(result).toEqual(3);
    });

    it('should return 1 when no attributes exist', () =>
    {
      const result = getNextIndex(noProtoExisting, Proto.PROPERTY_ATTRIBUTE_NAME);

      // Assert
      expect(result).toEqual(1);
    });

  });

  describe('hasAttribute', () =>
  {
    it('should return true when attribute exists', () =>
    {
      const result = hasAttribute(protoExisting, Proto.PROPERTY_ATTRIBUTE_NAME);

      // Assert
      expect(result).toBeTruthy();
    });

    it('should return false when no attribute', () =>
    {
      const result = hasAttribute(noProtoExisting, Proto.PROPERTY_ATTRIBUTE_NAME);

      // Assert
      expect(result).toBeFalsy();
    });

  });

  describe('getNextDataContractIndex', () =>
  {
    it('should return next index from existing', () =>
    {
      const result = getNextIndex(dataContractExisting, Data.PROPERTY_ATTRIBUTE_NAME);

      // Assert
      expect(result).toEqual(3);
    });

    it('should return 1 when no attributes exist', () =>
    {
      const result = getNextIndex(noDataContractExisting, Data.PROPERTY_ATTRIBUTE_NAME);

      // Assert
      expect(result).toEqual(1);
    });

  });

  describe('handlePropertyAttributeReorder', () =>
  {
    it('should reorder', () =>
    {
      const result = handlePropertyAttributeReorder(protoReorderExisting, Proto.PROPERTY_ATTRIBUTE_NAME);

      // Assert
      expect(result).toEqual(protoReorderExistingExpected);
    });

    it('should return text unchanged when no attributes exist', () =>
    {
      const result = handlePropertyAttributeReorder(noProtoExisting, Proto.PROPERTY_ATTRIBUTE_NAME);
      expect(result).toEqual(noProtoExisting);
    });
  });

  describe('hasAttributeInLines', () =>
  {
    it('should return false when lines is null', () =>
    {
      expect(hasAttributeInLines(null, 'ProtoMember')).toBe(false);
    });

    it('should return true when attribute found in lines', () =>
    {
      expect(hasAttributeInLines(['[ProtoMember(1)]', 'other line'], 'ProtoMember')).toBe(true);
    });

    it('should return false when attribute not in lines', () =>
    {
      expect(hasAttributeInLines(['[DataMember(Order=1)]', 'other line'], 'ProtoMember')).toBe(false);
    });

    it('should return false for empty array', () =>
    {
      expect(hasAttributeInLines([], 'ProtoMember')).toBe(false);
    });
  });

  describe('getAllAttributes', () =>
  {
    it('should return all matching attributes', () =>
    {
      const text = '[ProtoMember(1)]\npublic int A { get; set; }\n[ProtoMember(2)]\npublic int B { get; set; }';
      const result = getAllAttributes(text, 'ProtoMember');
      expect(result).toHaveLength(2);
      expect(result[0][1]).toBe('1');
      expect(result[1][1]).toBe('2');
    });

    it('should return empty array when no attributes exist', () =>
    {
      const result = getAllAttributes('public int A { get; set; }', 'ProtoMember');
      expect(result).toHaveLength(0);
    });
  });

  describe('addAttributeToDocument', () =>
  {
    it('should insert attribute before signature with correct indentation', () =>
    {
      const text = '    public int MyProp { get; set; }';
      const sig = new SignatureLineResult('public int MyProp { get; set; }', SignatureType.FullProperty, 0);
      sig.defaultLineIndent = 4;
      const result = addAttributeToDocument('\n', text, sig, '[ProtoMember(1)]');
      expect(result).toBe('    [ProtoMember(1)]\n    public int MyProp { get; set; }');
    });

    it('should return text unchanged when signature is null', () =>
    {
      const text = 'some text';
      const sig = new SignatureLineResult(null, SignatureType.FullProperty, 0);
      sig.defaultLineIndent = 0;
      const result = addAttributeToDocument('\n', text, sig, '[ProtoMember(1)]');
      expect(result).toBe('some text');
    });

    it('should work with CRLF line endings', () =>
    {
      const text = '    public int MyProp { get; set; }';
      const sig = new SignatureLineResult('public int MyProp { get; set; }', SignatureType.FullProperty, 0);
      sig.defaultLineIndent = 4;
      const result = addAttributeToDocument('\r\n', text, sig, '[ProtoMember(1)]');
      expect(result).toBe('    [ProtoMember(1)]\r\n    public int MyProp { get; set; }');
    });
  });

  describe('addUsingsToDocument', () =>
  {
    it('should add new using to document', () =>
    {
      const text = 'using System;\n\npublic class Foo {}';
      const result = addUsingsToDocument('\n', text, ['using ProtoBuf;']);
      expect(result).toContain('using ProtoBuf;');
      expect(result).toContain('using System;');
    });

    it('should deduplicate when using already exists', () =>
    {
      const text = 'using System;\nusing ProtoBuf;\n\npublic class Foo {}';
      const result = addUsingsToDocument('\n', text, ['using ProtoBuf;']);
      const count = (result.match(/using ProtoBuf;/g) || []).length;
      expect(count).toBe(1);
    });

    it('should return empty string unchanged when content is empty', () =>
    {
      const result = addUsingsToDocument('\n', '', ['using ProtoBuf;']);
      expect(result).toBe('');
    });
  });

  describe('removeUsingsFromDocument', () =>
  {
    it('should remove specified using statement', () =>
    {
      const text = 'using System;\nusing ProtoBuf;\n\npublic class Foo {}';
      const result = removeUsingsFromDocument('\n', text, ['using ProtoBuf;']);
      expect(result).not.toContain('using ProtoBuf;');
      expect(result).toContain('using System;');
    });

    it('should remove multiple using statements', () =>
    {
      const text = 'using System;\nusing ProtoBuf;\nusing System.Runtime.Serialization;\n\npublic class Foo {}';
      const result = removeUsingsFromDocument('\n', text, ['using ProtoBuf;', 'using System.Runtime.Serialization;']);
      expect(result).not.toContain('using ProtoBuf;');
      expect(result).not.toContain('using System.Runtime.Serialization;');
      expect(result).toContain('using System;');
    });

    it('should return empty string unchanged when content is empty', () =>
    {
      const result = removeUsingsFromDocument('\n', '', ['using ProtoBuf;']);
      expect(result).toBe('');
    });
  });

  describe('removeClassAttributeFromDocument', () =>
  {
    it('should remove class-level attribute', () =>
    {
      const text = '[ProtoContract]\npublic class Foo {}';
      const result = removeClassAttributeFromDocument('\n', text, 'ProtoContract');
      expect(result).not.toContain('[ProtoContract]');
      expect(result).toContain('public class Foo {}');
    });

    it('should remove indented class-level attribute', () =>
    {
      const text = '    [ProtoContract]\n    public class Foo {}';
      const result = removeClassAttributeFromDocument('\n', text, 'ProtoContract');
      expect(result).not.toContain('[ProtoContract]');
    });

    it('should return empty string unchanged when content is empty', () =>
    {
      const result = removeClassAttributeFromDocument('\n', '', 'ProtoContract');
      expect(result).toBe('');
    });
  });

  describe('removePropertyAttributeFromDocument', () =>
  {
    it('should remove property attribute with arguments', () =>
    {
      const text = '    [ProtoMember(1)]\n    public int MyProp { get; set; }';
      const result = removePropertyAttributeFromDocument('\n', text, 'ProtoMember');
      expect(result).not.toContain('[ProtoMember(1)]');
      expect(result).toContain('public int MyProp { get; set; }');
    });

    it('should remove all matching property attributes', () =>
    {
      const text = '    [ProtoMember(1)]\n    public int A { get; set; }\n    [ProtoMember(2)]\n    public int B { get; set; }';
      const result = removePropertyAttributeFromDocument('\n', text, 'ProtoMember');
      expect(result).not.toContain('[ProtoMember');
    });

    it('should return empty string unchanged when content is empty', () =>
    {
      const result = removePropertyAttributeFromDocument('\n', '', 'ProtoMember');
      expect(result).toBe('');
    });
  });

  describe('handleClassAttributes', () =>
  {
    it('should add class attribute when not present in leading trivia', () =>
    {
      const sig = new SignatureLineResult('public class Foo {', SignatureType.Class, 0);
      sig.leadingTrivia = [];
      sig.defaultLineIndent = 0;
      const text = 'public class Foo {';
      const result = handleClassAttributes(sig, '\n', text, 'ProtoContract', '[ProtoContract]');
      expect(result).toContain('[ProtoContract]');
    });

    it('should not add class attribute when already in leading trivia', () =>
    {
      const sig = new SignatureLineResult('public class Foo {', SignatureType.Class, 0);
      sig.leadingTrivia = ['[ProtoContract]'];
      sig.defaultLineIndent = 0;
      const text = '[ProtoContract]\npublic class Foo {';
      const result = handleClassAttributes(sig, '\n', text, 'ProtoContract', '[ProtoContract]');
      const count = (result.match(/\[ProtoContract\]/g) || []).length;
      expect(count).toBe(1);
    });
  });

  describe('handlePropertyAttributes', () =>
  {
    it('should add attribute and invoke callback when neither attribute nor ignore present', () =>
    {
      const sig = new SignatureLineResult('public int MyProp { get; set; }', SignatureType.FullProperty, 0);
      sig.leadingTrivia = [];
      sig.defaultLineIndent = 0;
      const text = 'public int MyProp { get; set; }';
      const callback = jest.fn();
      const result = handlePropertyAttributes(sig, '\n', text, 'ProtoMember', '[ProtoMember(1)]', 'ProtoIgnore', callback);
      expect(result).toContain('[ProtoMember(1)]');
      expect(callback).toHaveBeenCalledTimes(1);
    });

    it('should skip and not invoke callback when attribute already in leading trivia', () =>
    {
      const sig = new SignatureLineResult('public int MyProp { get; set; }', SignatureType.FullProperty, 0);
      sig.leadingTrivia = ['[ProtoMember(1)]'];
      sig.defaultLineIndent = 0;
      const text = '[ProtoMember(1)]\npublic int MyProp { get; set; }';
      const callback = jest.fn();
      handlePropertyAttributes(sig, '\n', text, 'ProtoMember', '[ProtoMember(2)]', 'ProtoIgnore', callback);
      expect(callback).not.toHaveBeenCalled();
    });

    it('should skip and not invoke callback when ignore attribute in leading trivia', () =>
    {
      const sig = new SignatureLineResult('public int MyProp { get; set; }', SignatureType.FullProperty, 0);
      sig.leadingTrivia = ['[ProtoIgnore]'];
      sig.defaultLineIndent = 0;
      const text = '[ProtoIgnore]\npublic int MyProp { get; set; }';
      const callback = jest.fn();
      const result = handlePropertyAttributes(sig, '\n', text, 'ProtoMember', '[ProtoMember(1)]', 'ProtoIgnore', callback);
      expect(callback).not.toHaveBeenCalled();
      expect(result).not.toContain('[ProtoMember(1)]');
    });
  });

  describe('handleEnumAttributes', () =>
  {
    it('should add contract attribute to enum and member attribute to each value', () =>
    {
      const sig = new SignatureLineResult('public enum MyEnum', SignatureType.Enum, 5);
      sig.leadingTrivia = [];
      sig.defaultLineIndent = 2;
      const result = handleEnumAttributes(sig, '\n', protoEnum, 'ProtoContract', '[ProtoContract]', 'ProtoEnum', '[ProtoEnum]');
      expect(result).toContain('[ProtoContract]');
      const memberCount = (result.match(/\[ProtoEnum\]/g) || []).length;
      expect(memberCount).toBe(3);
    });

    it('should not add contract attribute when already in leading trivia', () =>
    {
      const enumWithContract = `namespace Sample
{
  [ProtoContract]
  public enum MyEnum
  {
      One,
      Two
  }
}`;
      const sig = new SignatureLineResult('public enum MyEnum', SignatureType.Enum, 4);
      sig.leadingTrivia = ['[ProtoContract]'];
      sig.defaultLineIndent = 2;
      const result = handleEnumAttributes(sig, '\n', enumWithContract, 'ProtoContract', '[ProtoContract]', 'ProtoEnum', '[ProtoEnum]');
      const contractCount = (result.match(/\[ProtoContract\]/g) || []).length;
      expect(contractCount).toBe(1);
    });
  });
});


