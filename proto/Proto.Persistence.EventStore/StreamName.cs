namespace Proto.Persistence.EventStore
{
    public delegate string StreamNameStrategy(string actorName);
    
    public static class DefaultStrategy
    {
        public static string DefaultEventStreamNameStrategy(string actorName) => actorName;
        
        public static string DefaultSnapshotStreamNameStrategy(string actorName) => 
            "snapshot-" + actorName;
    }
    
}