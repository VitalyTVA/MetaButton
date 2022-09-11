using System.Reflection;
using ThatButtonAgain;

public class ThatButtonSketches : ISkecthesProvider {
    ICollection<SketchGroup> ISkecthesProvider.Groups => new[] {
        new SketchGroup {
            Name = "That Button Again",
            Sketches = GameController.Levels
                .Select((x, i) => new SkecthInfo(
                    typeof(Level), 
                    parameters: new object[] { i }, 
                    name: i + " - " + x.name
                ))
                .ToArray()
        },
    };
}