using UUIDGenerator;

namespace UUIDGenerator.Tests
{
    public class UuidGeneratorTests
    {
        private readonly IUuidGenerator _uuidGenerator;

        public UuidGeneratorTests()
        {
            _uuidGenerator = new UuidGenerator();
        }

        [Fact]
        public void GenerateUuid_ReturnsNonEmptyString()
        {
            var result = _uuidGenerator.GenerateUuid();

            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void GenerateUuid_ReturnsParsableGuid()
        {
            var result = _uuidGenerator.GenerateUuid();

            Assert.True(Guid.TryParse(result, out _));
        }
    }
}
