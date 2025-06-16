using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using SharedComponents.Configuration;
using SharedComponents.Pages.LearningApps.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Pages.LearningApps.Model
{
    public class LearningAppCardBase : ComponentBase
    {
        [Parameter]
        public LearningAppData Data { get; set; } = default!;

        [Parameter]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected LearningAppCardController Controller { get; set; } = default!;

        public LearningAppCardBase()
        {
            Controller = new LearningAppCardController(this);
        }
    }
}
