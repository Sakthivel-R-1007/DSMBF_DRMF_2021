namespace DSMBF_DRMF.Domain
{
    public class Category : Entity<long>
    {
        public string Name { get; set; }
        public bool Value { get; set; }
    }
}
