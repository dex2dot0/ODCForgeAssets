using OutSystems.ExternalLibraries.SDK;

namespace UUIDGenerator
{
    /// <summary>
    /// The UuidGenerator class exposes UUID generation to OutSystems.
    /// </summary>
    public class UuidGenerator : IUuidGenerator { }

    [OSInterface(Description = "Generates a random UUID", Name = "UUIDGenerator")]
    public interface IUuidGenerator
    {
        [OSAction(
            Description = "Generates a new UUID string",
            ReturnDescription = "A newly generated UUID string",
            ReturnName = "UUID")]
        public string GenerateUuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
