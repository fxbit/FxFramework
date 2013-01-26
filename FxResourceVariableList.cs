using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace FXFramework
{
    class FXResourceVariableItem
    {
        public FXResourceVariable resource;
        public String Name;
    }


    public class FxResourceVariableList
    {

        /// <summary>
        /// List that store all the resource variables
        /// </summary>
        List<FXResourceVariableItem> ListWithResourceVariables;

        public FxResourceVariableList()
        {
            // allocate the list
            ListWithResourceVariables = new List<FXResourceVariableItem>();
        }




        #region Add Resources

        /// <summary>
        /// Add the resource from shader base on the name
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="Name"></param>
        public void AddResourceFromShader(FXEffect effect, String Name)
        {
            FXResourceVariableItem newItem = new FXResourceVariableItem();

            // set the name of the item
            newItem.Name = Name;

            // get the item from the effect
            newItem.resource = effect.GetResourceByName(Name);

            // add the new item to the list
            ListWithResourceVariables.Add(newItem);
        } 
        #endregion




        #region Set views

        /// <summary>
        /// Bind all the sub resources to view
        /// </summary>
        /// <param name="view"></param>
        public void SetViewsToResources(ShaderResourceView view)
        {
            // bind all the sub-resources with the same view
            foreach (var resource in ListWithResourceVariables)
            {
                resource.resource.SetResource(view);
            }
        }

        /// <summary>
        /// Bind all the sub resources to view
        /// </summary>
        /// <param name="view"></param>
        public void SetViewsToResources(UnorderedAccessView view)
        {
            // bind all the sub-resources with the same view
            foreach (var resource in ListWithResourceVariables)
            {
                resource.resource.SetResource(view);
            }
        } 
        #endregion
    }
}
