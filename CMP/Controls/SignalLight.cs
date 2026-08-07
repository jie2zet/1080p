using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CMP.Controls;

/// <summary>
/// 现代风格LED信号灯控件
/// 支持红、黄、蓝、绿四种颜色，亮/灭状态，以及呼吸灯效果
/// </summary>
public class SignalLight : Control
{
    #region 依赖属性

    public static readonly DependencyProperty LightColorProperty =
        DependencyProperty.Register(nameof(LightColor), typeof(SignalLightColor), typeof(SignalLight),
            new PropertyMetadata(SignalLightColor.Blue, OnAppearanceChanged));

    public SignalLightColor LightColor
    {
        get => (SignalLightColor)GetValue(LightColorProperty);
        set => SetValue(LightColorProperty, value);
    }

    public static readonly DependencyProperty IsOnProperty =
        DependencyProperty.Register(nameof(IsOn), typeof(bool), typeof(SignalLight),
            new PropertyMetadata(false, OnIsOnChanged));

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public static readonly DependencyProperty IsBreathingProperty =
        DependencyProperty.Register(nameof(IsBreathing), typeof(bool), typeof(SignalLight),
            new PropertyMetadata(false, OnBreathingChanged));

    public bool IsBreathing
    {
        get => (bool)GetValue(IsBreathingProperty);
        set => SetValue(IsBreathingProperty, value);
    }

    public static readonly DependencyProperty BreathingDurationProperty =
        DependencyProperty.Register(nameof(BreathingDuration), typeof(double), typeof(SignalLight),
            new PropertyMetadata(1.2, OnBreathingChanged));

    public double BreathingDuration
    {
        get => (double)GetValue(BreathingDurationProperty);
        set => SetValue(BreathingDurationProperty, value);
    }

    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register(nameof(TransitionDuration), typeof(double), typeof(SignalLight),
            new PropertyMetadata(0.2));

    public double TransitionDuration
    {
        get => (double)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    #endregion

    static SignalLight()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SignalLight),
            new FrameworkPropertyMetadata(typeof(SignalLight)));
    }

    private Ellipse? _lightBodyOn;
    private Ellipse? _lightBodyOff;
    private Ellipse? _glowLayer;
    private Storyboard? _breathingStoryboard;
    private Storyboard? _transitionStoryboard;
    private bool _isTemplateApplied;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _lightBodyOn = GetTemplateChild("PART_LightBodyOn") as Ellipse;
        _lightBodyOff = GetTemplateChild("PART_LightBodyOff") as Ellipse;
        _glowLayer = GetTemplateChild("PART_GlowLayer") as Ellipse;
        _isTemplateApplied = true;

        SetupColors();
        SetLightStateImmediate();

        if (IsOn && IsBreathing)
        {
            StartBreathingAnimation();
        }
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SignalLight light && light._isTemplateApplied)
        {
            light.SetupColors();
        }
    }

    private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SignalLight light && light._isTemplateApplied)
        {
            light.AnimateToState();
        }
    }

    private static void OnBreathingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SignalLight light && light._isTemplateApplied)
        {
            if (light.IsBreathing && light.IsOn)
            {
                light.StartBreathingAnimation();
            }
            else
            {
                light.StopBreathingAnimation();
            }
        }
    }

    private void SetupColors()
    {
        var onColors = GetColorSet(LightColor, true);
        var offColors = GetColorSet(LightColor, false);

        if (_lightBodyOn != null)
        {
            _lightBodyOn.Fill = CreateLightBrush(onColors);
        }

        if (_lightBodyOff != null)
        {
            _lightBodyOff.Fill = CreateLightBrush(offColors);
        }

        if (_glowLayer != null)
        {
            var glowBrush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5)
            };
            glowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, onColors.Main.R, onColors.Main.G, onColors.Main.B), 0));
            glowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(80, onColors.Main.R, onColors.Main.G, onColors.Main.B), 0.5));
            glowBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, onColors.Main.R, onColors.Main.G, onColors.Main.B), 1));
            _glowLayer.Fill = glowBrush;
        }
    }

    private static RadialGradientBrush CreateLightBrush((Color Center, Color Main, Color Edge) colors)
    {
        var brush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.3, 0.3),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.55,
            RadiusY = 0.55
        };
        brush.GradientStops.Add(new GradientStop(colors.Center, 0));
        brush.GradientStops.Add(new GradientStop(colors.Main, 0.6));
        brush.GradientStops.Add(new GradientStop(colors.Edge, 1));
        return brush;
    }

    private void SetLightStateImmediate()
    {
        if (_lightBodyOn == null || _lightBodyOff == null || _glowLayer == null) return;

        _lightBodyOn.Opacity = IsOn ? 1.0 : 0.0;
        _lightBodyOff.Opacity = IsOn ? 0.0 : 1.0;
        _glowLayer.Opacity = IsOn ? 1.0 : 0.0;
    }

    private void AnimateToState()
    {
        if (_lightBodyOn == null || _lightBodyOff == null || _glowLayer == null) return;

        // 停止所有动画
        StopAllAnimations();

        var duration = TimeSpan.FromSeconds(TransitionDuration);
        var easing = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

        _transitionStoryboard = new Storyboard();

        // 亮灯层动画
        var onAnimation = new DoubleAnimation
        {
            To = IsOn ? 1.0 : 0.0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(onAnimation, _lightBodyOn);
        Storyboard.SetTargetProperty(onAnimation, new PropertyPath(OpacityProperty));
        _transitionStoryboard.Children.Add(onAnimation);

        // 灭灯层动画
        var offAnimation = new DoubleAnimation
        {
            To = IsOn ? 0.0 : 1.0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(offAnimation, _lightBodyOff);
        Storyboard.SetTargetProperty(offAnimation, new PropertyPath(OpacityProperty));
        _transitionStoryboard.Children.Add(offAnimation);

        // 发光层动画
        var glowAnimation = new DoubleAnimation
        {
            To = IsOn ? 1.0 : 0.0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(glowAnimation, _glowLayer);
        Storyboard.SetTargetProperty(glowAnimation, new PropertyPath(OpacityProperty));
        _transitionStoryboard.Children.Add(glowAnimation);

        // 过渡完成后的处理
        _transitionStoryboard.Completed += (s, e) =>
        {
            _transitionStoryboard = null;
            // 如果亮起且开启呼吸灯，启动呼吸动画
            if (IsOn && IsBreathing)
            {
                StartBreathingAnimation();
            }
        };

        _transitionStoryboard.Begin();
    }

    private void StopAllAnimations()
    {
        // 停止过渡动画
        if (_transitionStoryboard != null)
        {
            _transitionStoryboard.Stop();
            _transitionStoryboard = null;
        }

        // 停止呼吸动画
        StopBreathingAnimation();
    }

    private void StopBreathingAnimation()
    {
        if (_breathingStoryboard != null)
        {
            // 获取当前透明度值
            var currentOnOpacity = _lightBodyOn?.Opacity ?? 1.0;
            var currentGlowOpacity = _glowLayer?.Opacity ?? 1.0;

            _breathingStoryboard.Stop();
            _breathingStoryboard = null;

            // 恢复到当前值（避免跳变）
            if (_lightBodyOn != null) _lightBodyOn.Opacity = currentOnOpacity;
            if (_glowLayer != null) _glowLayer.Opacity = currentGlowOpacity;
        }
    }

    private void StartBreathingAnimation()
    {
        if (_lightBodyOn == null || _glowLayer == null || !IsOn || !IsBreathing) return;

        // 如果已有呼吸动画在运行，不重复启动
        if (_breathingStoryboard != null) return;

        _breathingStoryboard = new Storyboard();

        // 灯体呼吸动画 - 从当前值开始
        var bodyAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.5,
            Duration = TimeSpan.FromSeconds(BreathingDuration / 2),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(bodyAnimation, _lightBodyOn);
        Storyboard.SetTargetProperty(bodyAnimation, new PropertyPath(OpacityProperty));
        _breathingStoryboard.Children.Add(bodyAnimation);

        // 发光层呼吸动画
        var glowAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.4,
            Duration = TimeSpan.FromSeconds(BreathingDuration / 2),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(glowAnimation, _glowLayer);
        Storyboard.SetTargetProperty(glowAnimation, new PropertyPath(OpacityProperty));
        _breathingStoryboard.Children.Add(glowAnimation);

        _breathingStoryboard.Begin();
    }

    private static (Color Center, Color Main, Color Edge) GetColorSet(SignalLightColor color, bool isOn)
    {
        return color switch
        {
            SignalLightColor.Red => isOn
                ? (Color.FromRgb(255, 220, 220), Color.FromRgb(239, 68, 68), Color.FromRgb(185, 28, 28))
                : (Color.FromRgb(90, 60, 60), Color.FromRgb(60, 35, 35), Color.FromRgb(40, 22, 22)),

            SignalLightColor.Yellow => isOn
                ? (Color.FromRgb(255, 250, 220), Color.FromRgb(251, 191, 36), Color.FromRgb(217, 119, 6))
                : (Color.FromRgb(95, 85, 60), Color.FromRgb(65, 58, 35), Color.FromRgb(45, 40, 25)),

            SignalLightColor.Green => isOn
                ? (Color.FromRgb(220, 255, 220), Color.FromRgb(34, 197, 94), Color.FromRgb(22, 163, 74))
                : (Color.FromRgb(60, 90, 65), Color.FromRgb(35, 60, 40), Color.FromRgb(22, 42, 28)),

            SignalLightColor.Blue or _ => isOn
                ? (Color.FromRgb(220, 235, 255), Color.FromRgb(59, 130, 246), Color.FromRgb(37, 99, 235))
                : (Color.FromRgb(60, 75, 90), Color.FromRgb(35, 48, 62), Color.FromRgb(22, 32, 45)),
        };
    }
}

public enum SignalLightColor
{
    Red,
    Yellow,
    Green,
    Blue
}
