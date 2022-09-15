using MetaArt.Core;
using Svg.Skia;
using System.Diagnostics;
using System.Numerics;

namespace ThatButtonAgain;

public class Level {

    readonly int levelIndex;

    GameController controller = null!;

    public Level(int levelIndex) {
        this.levelIndex = levelIndex;
    }

    class SkiaSvgDrawing : SvgDrawing {
        public readonly SKSvg Svg;
        public SkiaSvgDrawing(SKSvg svg) {
            Svg = svg;
        }
    }
    class SketchSound : Sound {
        readonly SoundFile sound;
        public SketchSound(SoundFile sound) {
            this.sound = sound;
        }
        public override void Play() => sound.play();
    }

    void setup() {
        if(deviceType() == DeviceType.Desktop)
            size(400, 700);
        fullRedraw();
        
        rectMode(CORNER);
        controller = new GameController(
            width / displayDensity(),
            height / displayDensity(),
            createSound: stream => new SketchSound(createSound(stream)),
            createSvg: stream => {
                var svg = new Svg.Skia.SKSvg();
                svg.Load(stream);
                if(svg.Picture!.CullRect.Location != SKPoint.Empty)
                    throw new InvalidOperationException();
                return new SkiaSvgDrawing(svg);
            },
            levelIndex: levelIndex
        );


        textFont(createFont("SourceCodePro-Regular.ttf", controller.letterSize));
    }


    void draw()
    {
        controller.NextFrame(deltaTime);
        background(Colors.Background);
        scale(displayDensity(), displayDensity());
        foreach (var item in controller.scene.VisibleElements) {
            switch(item) {
                case Button b:
                    if(b.Filled) {
                        fill(b.IsPressed
                            ? (b.IsEnabled ? Colors.ButtonBackPressed : Colors.ButtonBackPressedDisabled)
                            : Colors.ButtonBackNormal);
                    } else {
                        noFill();
                    }
                    stroke(Colors.ButtonBorder);
                    strokeWeight(Constants.ButtonBorderWeight);
                    rect(item.Rect.Left, item.Rect.Top, item.Rect.Width, item.Rect.Height, Constants.ButtonCornerRadius);
                    break;
                case Letter l:
                    noStroke();
#if DEBUG
                    //if(MathF.VectorsEqual(l.Scale, Letter.NoScale)) {
                    fill(Colors.LetterDragBox);
                    rect(item.Rect.Left, item.Rect.Top, item.Rect.Width, item.Rect.Height);
                    //}
#endif
                    pushMatrix();
                    translate(item.Rect.MidX, item.Rect.MidY);
                    scale(l.Scale.X, l.Scale.Y);
                    rotate(l.Angle);
                    //fill(Colors.LetterDragBox);
                    //rect(0, 0, item.Rect.Width, item.Rect.Height);
                    var style = l.Style;
                    var letterColor = GetColorByStyle(style);
                    fill(color(
                        lerp(Colors.UIElementColor.Red, letterColor.Red, l.ActiveRatio),
                        lerp(Colors.UIElementColor.Green, letterColor.Green, l.ActiveRatio),
                        lerp(Colors.UIElementColor.Blue, letterColor.Blue, l.ActiveRatio),
                        l.Opacity * 255
                    ));
                    textAlign(TextAlign.CENTER, TextVerticalAlign.CENTER);
                    text(l.Value.ToString(), l.Offset.X, l.Offset.Y - controller.letterVerticalOffset);
                    //if(l.Value == 'C') {
                    //    scale(-1, 1);
                    //    text(l.Value.ToString(), -6, -controller.letterVerticalOffset);
                    //}
                    popMatrix();
                    break;
                case FadeOutElement f:
                    noStroke();
                    fill(0, f.Opacity);
                    rect(f.Rect.Left, f.Rect.Top, f.Rect.Width, f.Rect.Height);
                    break;
                case SvgElement s:
                    noStroke();
#if DEBUG
                    fill(Colors.LetterDragBox);
                    rect(item.Rect.Left, item.Rect.Top, item.Rect.Width, item.Rect.Height);
#endif
                    var svg = ((SkiaSvgDrawing)s.Svg).Svg;
                    float scaleX = (s.Size ?? s.Rect.Width) / svg.Picture!.CullRect.Width;
                    float scaleY = (s.Size ?? s.Rect.Height) / svg.Picture!.CullRect.Height;
                    var matrix = SKMatrix.CreateScale(scaleX, scaleY);
                    if(s.Size != null) {
                        matrix.TransX = (s.Rect.Width - s.Size.Value) / 2;
                        matrix.TransY = (s.Rect.Height - s.Size.Value) / 2;
                    }
                    pushMatrix();
                    translate(s.Rect.Left, s.Rect.Top);
                    SKPaint? paint = null;
                    if(s.Style != null) {
                        if(!svgPaints.TryGetValue(s.Style.Value, out paint)) {
                            svgPaints[s.Style.Value] = paint = new SKPaint { 
                                ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(GetColorByStyle(s.Style.Value).Value), SKBlendMode.SrcIn)
                            };
                        }
                    };
                    ((MetaArt.Skia.SkiaGraphics)MetaArt.Internal.Graphics.GraphicsInstance).Canvas.DrawPicture(svg.Picture, ref matrix, paint);
                    popMatrix();
                    break;
                case PathElement p:
                    shapeCorners(Constants.ButtonCornerRadius);
                    if(p.Filled) {
                        fill(Colors.ButtonBackNormal);
                    } else {
                        noFill();
                    }
                    stroke(Colors.ButtonBorder);
                    strokeWeight(Constants.ButtonBorderWeight);
                    beginShape();
                    foreach(var point in p.Points) {
                        vertex(point.X, point.Y);
                    }
                    endShape(EndShapeMode.CLOSE);
                    shapeCorners(0);
                    break;
                case BallElement b:
                    var strokeColor = b.State switch {
                        BallState.Active or BallState.Broken => Colors.ButtonBorder,
                        BallState.Disabled => Colors.UIElementColor,
                        _ => throw new InvalidOperationException(),
                    };
                    stroke(strokeColor);
                    strokeWeight(Constants.ButtonBorderWeight);
                    var fillColor = b.State switch {
                        BallState.Active => Colors.ButtonBackNormal,
                        BallState.Broken => Colors.AccentError,
                        BallState.Disabled => new Color(0),
                        _ => throw new InvalidOperationException(),
                    };
                    fill(fillColor);
                    ellipse(b.Rect.MidX, b.Rect.MidY, b.Rect.Width, b.Rect.Height);
                    break;
                case Line s:
                    stroke(Colors.AccentError);
                    strokeWeight(s.Thickness);
                    noFill();
                    line(s.From.X, s.From.Y, s.To.X, s.To.Y);
                    break;
                case InputHandlerElement i:
                    noStroke();
                    var _Color = Colors.Background;
                    fill(color(
                        _Color.Red,
                        _Color.Green,
                        _Color.Blue,
                        i.Opacity * 255
                    ));
                    rect(item.Rect.Left, item.Rect.Top, item.Rect.Width, item.Rect.Height);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        //if(isMousePressed || animations.HasAnimations)
        //    loop();
        //else
        //    noLoop();
    }

    private static Color GetColorByStyle(LetterStyle style) {
        return style switch {
            LetterStyle.Accent1 => Colors.LetterColor,
            LetterStyle.Accent2 => Colors.LetterColorAccent2,
            LetterStyle.Accent3 => Colors.LetterColorAccent3,
            LetterStyle.Accent4 => Colors.LetterColorAccent4,
            LetterStyle.Accent5 => Colors.LetterColorAccent5,
            LetterStyle.Inactive => Colors.UIElementColor,
            _ => throw new InvalidOperationException(),
        };
    }

    Dictionary<LetterStyle, SKPaint> svgPaints = new();

    void mousePressed()
    {
        controller.scene.Press(new System.Numerics.Vector2(mouseX / displayDensity(), mouseY / displayDensity()));
        //loop();
    }

    void mouseDragged()
    {
        controller.scene.Drag(new Vector2(mouseX / displayDensity(), mouseY / displayDensity()));
    }

    void mouseReleased()
    {
        //locked = false;
        controller.scene.Release(new System.Numerics.Vector2(mouseX / displayDensity(), mouseY / displayDensity()));
        //noLoop();
    }

    //static class Colors {
    //    public static Color LetterColor => new Color(0, 0, 0);
    //    public static Color UIElementColor => new Color(70, 70, 70);
    //    public static Color LetterDragBox => new Color(120, 0, 0, 10);
    //    public static Color Background => new Color(150, 150, 150);
    //    public static Color ButtonBackNormal => new Color(255, 255, 255);
    //    public static Color ButtonBackPressed => new Color(200, 200, 200);
    //    public static Color ButtonBackPressedDisabled => new Color(200, 0, 0);
    //}

    static class Colors {
        //static Palette Palette => Palettes.BlueGrayLight;
        static Palette Palette => Palettes.CoolGrayDark;

        public static Color LetterColor => Palette.AccentInfo;
        public static Color LetterColorAccent2 => Palette.AccentSecondary1;
        public static Color LetterColorAccent3 => Palette.AccentSuccess;
        public static Color LetterColorAccent4 => Palette.AccentSecondary2;
        public static Color LetterColorAccent5 => Palette.AccentError;
        public static Color UIElementColor => Palette._600;
        public static Color LetterDragBox => Color.Empty; //new Color(120, 0, 0, 50);
        //public static Color InputHandlerBox => Color.Empty; // new Color(120, 0, 0, 50);
        public static Color Background => Palette._50;
        public static Color ButtonBackNormal => Palette._100;
        public static Color ButtonBackPressed => Palette._200;
        public static Color ButtonBorder => Palette.AccentInfo;

        public static Color ButtonBackPressedDisabled => Palette.AccentError;
        public static Color AccentError => Palette.AccentError;
    }


    public static class CommonColors { 
        public static Color Light_AccentError => Color.FromRGBValue(0xC62828);
        public static Color Light_AccentInfo => Color.FromRGBValue(0x4527A0);
        public static Color Light_AccentSuccess => Color.FromRGBValue(0xA5D6A7);
        public static Color Light_AccentSecondary1 => Color.FromRGBValue(0x90CAF9);
        public static Color Light_AccentSecondary2 => Color.FromRGBValue(0xCE93D8);

        public static Color Dark_AccentError => Color.FromRGBValue(0xCF6679);
        public static Color Dark_AccentInfo => Color.FromRGBValue(0xEFC9A4);
        public static Color Dark_AccentSuccess => Color.FromRGBValue(0x1DDECB);
        public static Color Dark_AccentSecondary1 => Color.FromRGBValue(0xB3F5FF);
        public static Color Dark_AccentSecondary2 => Color.FromRGBValue(0xD6A9D5);
    }

    static class Palettes {
        //https://material.io/design/color/the-color-system.html#color-usage-and-palettes
        public static readonly Palette BlueGrayLight = new Palette {
            _50 = Color.FromRGBValue(0xECEFF1),
            _100 = Color.FromRGBValue(0xCFD8DC),
            _200 = Color.FromRGBValue(0xB0BEC5),
            _300 = Color.FromRGBValue(0x90A4AE),
            _400 = Color.FromRGBValue(0x78909C),
            _500 = Color.FromRGBValue(0x607D8B),
            _600 = Color.FromRGBValue(0x546E7A),
            _700 = Color.FromRGBValue(0x455A64),
            _800 = Color.FromRGBValue(0x37474F),
            _900 = Color.FromRGBValue(0x263238),
            AccentError = CommonColors.Light_AccentError,
            AccentInfo = CommonColors.Light_AccentInfo,
            AccentSuccess = CommonColors.Light_AccentSuccess,
            AccentSecondary1 = CommonColors.Light_AccentSecondary1,
            AccentSecondary2 = CommonColors.Light_AccentSecondary2,
        };

        //https://compilezero.medium.com/dark-mode-ui-design-the-definitive-guide-part-1-color-53dcfaea5129
        //https://material.io/design/color/dark-theme.html#ui-application
        public static readonly Palette CoolGrayDark = new Palette {
            _50 = Color.FromRGBValue(0x1F2933),
            _100 = Color.FromRGBValue(0x323F4B),
            _200 = Color.FromRGBValue(0x3E4C59),
            _300 = Color.FromRGBValue(0x52606D),
            _400 = Color.FromRGBValue(0x616E7C),
            _500 = Color.FromRGBValue(0x7B8794),
            _600 = Color.FromRGBValue(0x9AA5B1),
            _700 = Color.FromRGBValue(0xCBD2D9),
            _800 = Color.FromRGBValue(0xE4E7EB),
            _900 = Color.FromRGBValue(0xF5F7FA),
            AccentError = CommonColors.Dark_AccentError,
            AccentInfo = CommonColors.Dark_AccentInfo,
            AccentSuccess = CommonColors.Dark_AccentSuccess,
            AccentSecondary1 = CommonColors.Dark_AccentSecondary1,
            AccentSecondary2 = CommonColors.Dark_AccentSecondary2,
        };
    }

    class Palette {
        public Color _50 { get; init; }
        public Color _100 { get; init; }
        public Color _200 { get; init; }
        public Color _300 { get; init; }
        public Color _400 { get; init; }
        public Color _500 { get; init; }
        public Color _600 { get; init; }
        public Color _700 { get; init; }
        public Color _800 { get; init; }
        public Color _900 { get; init; }
        public Color AccentError { get; init; }
        public Color AccentInfo { get; init; }
        public Color AccentSuccess { get; init; }
        public Color AccentSecondary1 { get; init; }
        public Color AccentSecondary2 { get; init; }
    }
}

