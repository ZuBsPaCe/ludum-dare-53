using Godot;
using System.Collections.Generic;

public partial class Notification : CanvasLayer
{
	[Export] private Texture2D _infoTexture;
	[Export] private Texture2D _wonTexture;
	[Export] private Texture2D _lostTexture;

	private Control _root;
	private TextureRect _icon;
	private Label _label;

	private Tween _tween;

	private List<(NotificationType Type, string Message, bool Hold)> _queue = new();

	private bool _holding;

	private Vector2 _showPosition = new(0, 0);
	private Vector2 _hidePosition = new(0, 200);

	public override void _Ready()
	{
		_root = GetNode<Control>("Root");
		_icon = GetNode<TextureRect>("%Icon");
		_label = GetNode<Label>("%Label");

		_root.Position = _hidePosition;
		_root.Modulate = Colors.Transparent;
    }

	public void ShowNotification(NotificationType notificationType, string message, bool hold = false)
	{
		if (hold)
		{
            _queue.RemoveAll((item) => item.Hold);
        }

		_queue.Add((notificationType, message, hold));

		TryShowNext();
	}

	public void Unhold()
	{
		_queue.RemoveAll((item) => item.Hold);

		if (_holding)
		{
			_holding = false;

			_tween.Kill();
			_tween = CreateTween();

			_tween.TweenProperty(_root, "position", _hidePosition, 0.25);
			_tween.Parallel().TweenProperty(_root, "modulate", Colors.Transparent, 0.25);

			_tween.TweenCallback(Callable.From(() => { ShowNext(); }));
		}
		else
		{
			TryShowNext();
		}
	}

	private void TryShowNext()
	{
		if(_tween != null && _tween.IsRunning() || _queue.Count == 0)
		{
			return;
		}

		ShowNext();
	}

	private void ShowNext()
	{
		_tween?.Kill();
		_tween = null;

		if (_queue.Count == 0)
		{
			return;
		}

		var item = _queue[0];
		_queue.RemoveAt(0);

		_root.Position = new Vector2(0, 200);
		_root.Modulate = Colors.Transparent;

		_label.Text = item.Message;

        switch (item.Type)
        {
            case NotificationType.Info:
                _icon.Texture = _infoTexture;
                break;

            case NotificationType.Won:
                _icon.Texture = _wonTexture;
                break;

            case NotificationType.Lost:
                _icon.Texture = _lostTexture;
                break;

            default:
                Debug.Fail();
                break;
        }

        _tween = CreateTween();

		_tween.SetEase(Tween.EaseType.In);
		_tween.SetTrans(Tween.TransitionType.Cubic);

		_tween.TweenProperty(_root, "position", _showPosition, 0.25);
		_tween.Parallel().TweenProperty(_root, "modulate", Colors.White, 0.25);

		if (!item.Hold)
		{
			_tween.TweenInterval(2.0);

			_tween.TweenProperty(_root, "position", _hidePosition, 0.25);
			_tween.Parallel().TweenProperty(_root, "modulate", Colors.Transparent, 0.25);

			_tween.TweenInterval(0.5);

			_tween.TweenCallback(Callable.From(() => { ShowNext(); }));
		}
		else
		{
			_holding = true;
		}
    }
}
