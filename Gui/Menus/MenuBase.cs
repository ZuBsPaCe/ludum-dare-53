using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public abstract partial class MenuBase : CanvasLayer
{
    protected const float TRANSITION_DURATION = 0.4f;

    private List<Button> _allButtons = new();
    private Dictionary<Button, int> _buttonToState = new();

    private Button _currentButton;
    private Control _currentControl;

    private Control _buttonBar;
    private Tween _buttonBarTween;

    private bool _disabled = true;

    protected void InitButtonBar(Control buttonBar)
    {
        _buttonBar = buttonBar;
        _buttonBar.Position = new Vector2(1920, 0);
    }

    protected void InitButton<T>(Button button, T newState) where T : Enum
    {
        // https://stackoverflow.com/a/60022245/998987
        int intState = Unsafe.As<T, int>(ref newState);

        _allButtons.Add(button);
        _buttonToState[button] = intState;

        button.MouseEntered += () => Button_MouseEntered();
        button.Pressed += () => Button_Pressed(button);
    }

    protected void ShowButtonBar(float initialDelay = 0)
    {
        _buttonBar.Position = new Vector2(1920, 0);

        _buttonBarTween?.Kill();

        _buttonBarTween = _buttonBar.CreateTween();
        _buttonBarTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

        if (initialDelay > 0)
        {
            _buttonBarTween.TweenInterval(initialDelay);
        }

        _buttonBarTween.TweenProperty(_buttonBar, "position", new Vector2(1344, 0), TRANSITION_DURATION);

        _disabled = false;

        SetCurrentButton(_allButtons[0]);
    }

    private void Button_MouseEntered()
    {
        if (_disabled)
            return;

        Sounds.PlaySound(SoundType.MainMenuHover);
    }

    private void Button_Pressed(Button button)
    {
        if (_disabled)
            return;

        SetCurrentButton(button);

        MenuButtonPressed(_buttonToState[button]);
    }

    protected abstract MenuControlBase InstantiateMenuButtonControl(int state);
    protected abstract void MenuButtonPressed(int state);

    protected void CloseMenu(bool queueFree = true)
    {
        _disabled = true;

        SetCurrentButton(null);

        _buttonBarTween?.Kill();

        _buttonBarTween = _buttonBar.CreateTween();
        _buttonBarTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

        _buttonBarTween.TweenProperty(_buttonBar, "position", new Vector2(1920, 0), TRANSITION_DURATION);

        if (queueFree)
            _buttonBarTween.TweenCallback(Callable.From(() => QueueFree()));
    }

    private void SetCurrentButton(Button menuBarButton)
    {
        if (_currentButton == menuBarButton)
        {
            return;
        }

        if (_currentControl != null)
        {
            // Need a local vor for QueueFree()
            Control control = _currentControl;

            Tween hideTween = control.CreateTween();
            hideTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            hideTween.TweenProperty(control, "position", new Vector2(0, 1080), TRANSITION_DURATION);
            hideTween.Parallel().TweenProperty(control, "modulate", new Color(1, 1, 1, 0), TRANSITION_DURATION);
            hideTween.TweenCallback(Callable.From(() => control.QueueFree()));

            _currentControl = null;
        }

        _currentButton = menuBarButton;

        if (menuBarButton != null)
        {
            _currentButton.GrabFocus();

            Control nextControl = InstantiateMenuButtonControl(_buttonToState[menuBarButton]);

            if (nextControl != null)
            {
                _currentControl = nextControl;
                AddChild(_currentControl);

                List<Control> focusableControls = new();
                GetAllFocusableControls(_currentControl, focusableControls);

                foreach (Control leftControl in focusableControls)
                {
                    leftControl.VisibilityChanged += () =>
                    {
                        UpdateFocusToLeft(focusableControls, menuBarButton);
                    };
                }

                UpdateFocusToLeft(focusableControls, menuBarButton);

                _currentControl.Position = new Vector2(0, -1080);
                _currentControl.Modulate = new Color(1, 1, 1, 0);

                Tween show_tween = nextControl.CreateTween();
                show_tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                show_tween.TweenProperty(nextControl, "position", new Vector2(0, 0), TRANSITION_DURATION);
                show_tween.Parallel().TweenProperty(nextControl, "modulate", new Color(1, 1, 1, 1), TRANSITION_DURATION);

                Sounds.PlaySound(SoundType.MainMenuSelect);
            }
            else
            {
                menuBarButton.FocusNeighborLeft = null;
            }
        }
    }

    private void UpdateFocusToLeft(List<Control> focusableControls, Button menuBarButton)
    {
        Control firstFocusableControl = null;
        int visibleCount = 0;

        foreach (Control leftControl in focusableControls)
        {
            if (leftControl.IsVisibleInTree())
            {
                visibleCount += 1;

                if (firstFocusableControl == null)
                {
                    firstFocusableControl = leftControl;
                }
            }

            if (leftControl.FocusNeighborRight.IsEmpty)
            {
                leftControl.FocusNeighborRight = menuBarButton.GetPath();
            }
        }

        if (firstFocusableControl == null)
        {
            foreach (Button button in _allButtons)
            {
                button.FocusNeighborLeft = new NodePath();
            }
        }
        else
        {
            foreach (Button button in _allButtons)
            {
                button.FocusNeighborLeft = firstFocusableControl.GetPath();
            }
        }

        if (visibleCount == 0)
        {
            menuBarButton.GrabFocus();
        }
    }

    private void HideCurrentControl()
    {
        if (_currentControl == null)
            return;

        Control control = _currentControl;
        _currentControl = null;

        Tween tween = control.CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(control, "position", new Vector2(0, 1080), TRANSITION_DURATION);
        tween.Parallel().TweenProperty(control, "modulate", new Color(1, 1, 1, 0), TRANSITION_DURATION);
        tween.TweenCallback(Callable.From(() => control.QueueFree()));
    }

    private void GetAllFocusableControls(Control currentControl, List<Control> foundControls)
    {
        if (currentControl.FocusMode == Control.FocusModeEnum.All)
        {
            foundControls.Add(currentControl);
        }

        foreach (Control control in currentControl.GetChildren().Where(child => child is Control).Cast<Control>())
        { 
            GetAllFocusableControls(control, foundControls);
        }
    }
}
