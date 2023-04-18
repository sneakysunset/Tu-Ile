namespace ProjectDawn.SplitScreen
{
    public interface ISplitScreen
    {
        bool IsCreated { get; }
        VoronoiDiagram VoronoiDiagram { get; }
        void CreateScreens(in Translating translating, ref ScreenRegions screenRegions);
        void DrawDelaunayDual();
        void DrawRegions(BlendShape blendShape);
    }
}
