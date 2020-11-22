﻿using System.Diagnostics;

namespace PrismContextAware.ViewModels
{
    public class ViewAViewModel : ViewAwareStatusViewModel
    {
        protected override void OnLoaded()
        {
            Debug.WriteLine($"OnLoaded ViewA");
        }
        protected override void OnUnloaded()
        {
            Debug.WriteLine($"OnUnloaded ViewA");
        }
    }
}
