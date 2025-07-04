namespace SystemInstaller.Web.Data
{
    public class InstallationEnvironment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<InstallationTask> Tasks { get; set; } = new();
        // Weitere Eigenschaften wie Docker Compose Pfad, Variablen etc. können ergänzt werden
    }
}
