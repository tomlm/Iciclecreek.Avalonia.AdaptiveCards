// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Text;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveActionRenderer
    {
        public static Control Render(AdaptiveAction action, AdaptiveRenderContext context)
        {
            if (context.Config.SupportsInteractivity && context.ActionHandlers.IsSupported(action.GetType()))
            {
                var uiButton = CreateActionButton(action, context);
                uiButton.Click += (sender, e) =>
                {
                    context.InvokeAction(uiButton, new AdaptiveActionEventArgs(action));

                    // Prevent nested events from triggering
                    e.Handled = true;
                };
                return uiButton;
            }
            return null;
        }

        public static Button CreateActionButton(AdaptiveAction action, AdaptiveRenderContext context)
        {
            var sb = new StringBuilder(action.Style ?? "Default".ToLower());
            sb[0] = Char.ToUpper(sb[0]);
            var actionStyle = sb.ToString();

            ContainerStyleConfig? containerConfig = context.Config.ContainerStyles.Default;
            var uiButton = new Button()
            {
                BorderThickness = new Thickness(1),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(1),
                IsEnabled = action.IsEnabled
            };
            switch (actionStyle)
            {
                case "Positive":
                    containerConfig = context.Config.ContainerStyles.Accent;
                    uiButton.Background = context.GetColorBrush(containerConfig.BackgroundColor);
                    uiButton.Foreground = context.GetColorBrush(context.Config.ContainerStyles.Default.ForegroundColors.Accent.Default);
                    uiButton.BorderBrush = context.GetColorBrush(context.Config.ContainerStyles.Default.ForegroundColors.Accent.Default);
                    break;
                case "Destructive":
                    containerConfig = context.Config.ContainerStyles.Default;
                    uiButton.Background = context.GetColorBrush(containerConfig.BackgroundColor);
                    uiButton.Foreground = context.GetColorBrush(containerConfig.ForegroundColors.Attention.Default);
                    uiButton.BorderBrush = context.GetColorBrush(containerConfig.ForegroundColors.Attention.Default);
                    break;
                default:
                    containerConfig = context.Config.ContainerStyles.Default;
                    uiButton.Background = context.GetColorBrush(containerConfig.BackgroundColor);
                    uiButton.Foreground = context.GetColorBrush(containerConfig.ForegroundColors.Accent.Default);
                    uiButton.BorderBrush = context.GetColorBrush(containerConfig.ForegroundColors.Accent.Default);
                    break;
            }

            // Style = context.GetStyle($"Adaptive.{action.Type}"),
            uiButton.Classes.Add(actionStyle);

            var contentStackPanel = new StackPanel();

            if (!context.IsRenderingSelectAction)
            {
                // Only apply padding for normal card actions
                uiButton.Padding = new Thickness(6, 4, 6, 4);
            }
            else
            {
                // Remove any extra spacing for selectAction
                uiButton.Padding = new Thickness(0, 0, 0, 0);
                contentStackPanel.Margin = new Thickness(0, 0, 0, 0);
            }
            uiButton.Content = contentStackPanel;
            Control uiIcon = null;

            var uiTitle = new TextBlock
            {
                Text = action.Title,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = context.Config.GetFontSize(AdaptiveFontType.Default, AdaptiveTextSize.Default),
                // Style = context.GetStyle($"Adaptive.Action.Title")
            };

            if (action.IconUrl != null)
            {
                var actionsConfig = context.Config.Actions;

                var image = new AdaptiveImage(action.IconUrl)
                {
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                };
                uiIcon = AdaptiveImageRenderer.Render(image, context);
                if (actionsConfig.IconPlacement == IconPlacement.AboveTitle)
                {
                    contentStackPanel.Orientation = Orientation.Vertical;
                    uiIcon.Height = (double)actionsConfig.IconSize;
                }
                else
                {
                    contentStackPanel.Orientation = Orientation.Horizontal;
                    //Size the image to the textblock, wait until layout is complete (loaded event)
                    uiIcon.Loaded += (sender, e) =>
                    {
                        uiIcon.Height = uiTitle.Bounds.Height;
                    };
                }
                contentStackPanel.Children.Add(uiIcon);

                // Add spacing for the icon for horizontal actions
                if (actionsConfig.IconPlacement == IconPlacement.LeftOfTitle)
                {
                    int spacing = context.Config.GetSpacing(AdaptiveSpacing.Default);
                    var uiSep = new Grid
                    {
                        // Style = context.GetStyle($"Adaptive.VerticalSeparator"),
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = spacing,
                    };
                    contentStackPanel.Children.Add(uiSep);
                }
            }

            if (!context.IsRenderingSelectAction)
            {
                contentStackPanel.Children.Add(uiTitle);
            }
            else
            {
                uiButton.SetValue(ToolTip.TipProperty, action.Tooltip ?? action.Title);
            }

            string name = context.GetType().Name.Replace("Action", String.Empty);

            uiButton.Classes.Add(typeof(AdaptiveAction).Name);
            return uiButton;
        }

    }
}
