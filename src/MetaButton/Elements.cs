using MetaArt.Core;

namespace ThatButtonAgain;
public abstract class SvgDrawing { }
public abstract class Sound {
    public abstract void Play();
}
public class SvgElement : Element {
    public SvgDrawing Svg { get; set; } = null!;
    public float? Size { get; set; }
    public LetterStyle? Style { get; set; }
}

public class PathElement : Element {
    public readonly Vector2[] Points;
    public bool Filled { get; set; } = true;

    public PathElement(Vector2[] points) {
        Points = points;
    }
}

public class Button : Element {
    public bool IsEnabled { get; set; } = true;
    public bool IsPressed { get; set; }
    public bool Filled { get; set; } = true;
}

public enum LetterStyle {
    Accent1, Accent2, Accent3, Accent4, Accent5, Inactive
}
public class Letter : Element {
    public static Vector2 NoScale = new Vector2(1, 1);
    public Vector2 Scale { get; set; }
    public float Angle { get; set; }
    public Vector2 Offset { get; set; }
    public char Value { get; set; }
    public float ActiveRatio { get; set; } = 1;
    public float Opacity { get; set; } = 1;
    public LetterStyle Style { get; set; }

    public Letter() {
        Scale = NoScale;
    }
}

public enum BallState { Active, Broken, Disabled }
public class BallElement : Element {
    public BallState State { get; set; } = BallState.Active;

}

public class Line : Element {
    public Vector2 From { get; set; }
    public Vector2 To { get; set; }
    public float Thickness { get; set; } = 1;
}
