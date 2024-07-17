namespace OpenIATest.Model
{
    public class Delta
    {
        public string Role { get; set; }
        public Context Context { get; set; }
        public string Content { get; set; }
    }

    public class Context
    {
        public List<Citation> Citations { get; set; }
        public string Intent { get; set; }
    }

    public class Citation
    {
        public string Content { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Filepath { get; set; }
        public string ChunkId { get; set; }
    }

    public class Choice
    {
        public int Index { get; set; }
        public Delta Delta { get; set; }
        public bool EndTurn { get; set; }
        public string FinishReason { get; set; }
    }

    public class Event
    {
        public string Id { get; set; }
        public string Model { get; set; }
        public int Created { get; set; }
        public string Object { get; set; }
        public List<Choice> Choices { get; set; }
    }
}