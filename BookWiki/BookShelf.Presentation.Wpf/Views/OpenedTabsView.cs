﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BookWiki.Presentation.Wpf.Extensions;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    public class OpenedTabsView : StackPanel
    {
        public OpenedTabsView()
        {
            Background = Brushes.LightGray;
        }

        public void Start()
        {
            BooksApplication.Instance.ItemsListChanged += Render;
            Render();
        }

        public void Stop()
        {
            BooksApplication.Instance.ItemsListChanged -= Render;
        }

        private void Render()
        {
            foreach (Button button in Children)
            {
                button.Click -= ItemClicked;
            }

            Children.Clear();

            Render(BooksApplication.Instance.FileSystemWindow);

            foreach (var novelWindow in BooksApplication.Instance.OpenedNovels.OrderBy(x => x.Title))
            {
                Render(novelWindow);
            }

            foreach (var articleWindow in BooksApplication.Instance.OpenedArticles.OrderBy(x => x.Title))
            {
                Render(articleWindow);
            }

            foreach (var mapWindow in BooksApplication.Instance.OpenedMaps.OrderBy(x => x.Title))
            {
                Render(mapWindow);
            }
        }

        private void Render(Window window)
        {
            var button = new TabItemView(window.Title);
            button.Tag = window;
            Children.Add(button);

            button.Click += ItemClicked;
        }

        private void ItemClicked(object sender, RoutedEventArgs e)
        {
            var b = sender.CastTo<Button>();

            var w = b.Tag.CastTo<Window>();

            w.ActivateOrRestore();
        }

        public void ToggleVisibility(bool? toVisible = null)
        {
            if (toVisible == null)
            {
                if (Visibility == Visibility.Visible)
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Visibility = Visibility.Visible;
                }
            }
            else
            {
                Visibility = toVisible.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}