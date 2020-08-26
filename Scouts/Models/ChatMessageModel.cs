using MvvmHelpers;
using System;
using Xamarin.Forms;

namespace Scouts.Models
{
    public class ChatMessageModel : ObservableObject
    {
        static Random Random = new Random();
        
        string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        
        string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        string _firstLetter;
        public string FirstLetter
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_firstLetter))
                    return _firstLetter.ToLowerInvariant();

                _firstLetter = Username?.Length > 0 ? Username[0].ToString() : "?";
                return _firstLetter.ToLowerInvariant();
            }
            set => _firstLetter = value;
        }

        Color _color;
        public Color Color
        {
            get
            {
                if (_color != null && _color.A != 0)
                    return _color;

                _color = Color.FromRgb(Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255)).MultiplyAlpha(.9);
                return _color;
            }
            set => _color = value;
        }

        Color _backgroundColor;
        public Color BackgroundColor
        {
            get
            {
                if (_backgroundColor != null && _backgroundColor.A != 0)
                    return _backgroundColor;

                _backgroundColor = Color.MultiplyAlpha(.6);
                return _backgroundColor;
            }
            set => _backgroundColor = value;
        }
    }
}