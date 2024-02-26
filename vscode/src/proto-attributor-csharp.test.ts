import { getNextIndex, hasAttribute } from './proto-attributor-csharp';
import { noProtoExisting, protoExisting } from './test/proto-test-class';
import { dataContractExisting, noDataContractExisting } from './test/data-test-class';
import { Data, Proto } from './utils/constants';

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

});


