namespace Appeaser.Tests
{
    public class TestMediatorSettings : IMediatorSettings
    {
        public TestMediatorSettings()
        {
            WrapExceptions = new MediatorSettings().WrapExceptions;
        }

        public bool WrapExceptions { get; set; }
    }
}
