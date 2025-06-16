using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedComponents.Pages.LearningApps.Model;

namespace SharedComponents.Pages.LearningApps.Controller
{
    public class LearningAppCardController
    {
        private LearningAppCardBase Model { get; set; }

        public LearningAppCardController(LearningAppCardBase model)
        {
            Model = model;
        }

        /// <summary>
        /// Route to the LearningApp with the given ID
        /// </summary>
        /// <param name="learningAppID">ID (=Name of H5P Folder)</param>
        public void RouteToLearningApp(string learningAppID)
        {
            Model.NavigationManager!.NavigateTo("/LearningApps/" + learningAppID);
        }
    }
}
