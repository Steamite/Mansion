using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Editor
{
    public static class ListViewUtility
    {
        public static void InitStyles(this ListView view, string name)
        {
            view.allowAdd = true;
            view.allowRemove = true;
            view.showAddRemoveFooter = true;
            view.showBorder = true;

            view.headerTitle = name;
            view.showFoldoutHeader = true;
        }
    }
}
