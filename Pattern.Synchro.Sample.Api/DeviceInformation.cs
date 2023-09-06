using Pattern.Synchro.Api;

namespace Pattern.Synchro.Sample.Api
{
    public class DeviceInformation: DeviceInformation<SampleDbContext, Device>
    {
        public DeviceInformation(SampleDbContext db) : base(db)
        {
        }

        protected override Device Create()
        {
            return new Device();
        }
    }
}