﻿using System.Windows.Input;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using hap.Models;
using hap.Services.Interfaces;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Screen = Caliburn.Micro.Screen;

namespace hap.ViewModels
{
    internal class OverlayViewModel : Screen
    {
        private Rect _bounds;
        private ObservableCollection<HintViewModel> _hints = new ObservableCollection<HintViewModel>();

        public OverlayViewModel(
            HintSession session,
            IHintLabelService hintLabelService)
        {
            _bounds = session.OwningWindowBounds;

            var labels = hintLabelService.GetHintStrings(session.Hints.Count());
            for (int i = 0; i < labels.Count; ++i)
            {
                var hint = session.Hints[i];
                _hints.Add(new HintViewModel(hint)
                {
                    Label = labels[i] + hint.AccessKey,
                    Active = false
                });
            }
        }

        /// <summary>
        /// Bounds in logical screen coordiantes
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                _bounds = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<HintViewModel> Hints
        {
            get
            {
                return _hints;
            }
            set
            {
                _hints = value;
                NotifyOfPropertyChange();
            }
        }

        public string MatchString
        {
            set
            {
                Hints.Apply(x => x.Active = false);

                var matching = Hints.Where(x => x.Label.StartsWith(value)).ToArray();
                matching.Apply(x => x.Active = true);

                if (matching.Count() == 1)
                {
                    matching.First().Hint.Invoke();
                    TryClose();
                }
            }
        }

        public void PreviewKey(KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                TryClose();
            }
        }
    }
}
