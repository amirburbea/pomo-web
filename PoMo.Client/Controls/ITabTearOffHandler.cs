using System.Windows;
using System.Windows.Controls;

namespace PoMo.Client.Controls
{
    /// <summary>
    /// Interface for handling the application specific handling of the tear off process.
    /// </summary>
    public interface ITabTearOffHandler
    {
        /// <summary>
        /// Queries if the application allows for a reorder attempt within the <paramref name="tabControl"/>.
        /// </summary>
        /// <param name="item">The item associated with the <see cref="T:System.Windows.Controls.TabItem"/> being dragged.</param>
        /// <param name="tabControl">The source tab control.</param>
        /// <param name="sourceIndex">Index of the <paramref name="item"/> in the <paramref name="tabControl"/>.</param>
        /// <param name="insertionIndex">Insertion index in the <paramref name="tabControl"/>.</param>
        bool AllowReorder(object item, TabControl tabControl, int sourceIndex, int insertionIndex);

        /// <summary>
        /// Queries if the application allows for the targeted drop (this represents a case when a tab is dropped onto another tab control).
        /// </summary>
        /// <param name="item">The item associated with the <see cref="T:System.Windows.Controls.TabItem"/> being dropped.</param>
        /// <param name="sourceTabControl">The source tab control.</param>
        /// <param name="sourceIndex">Index of the <paramref name="item"/> in the <paramref name="sourceTabControl"/>.</param>
        /// <param name="targetTabControl">The target tab control.</param>
        /// <param name="insertionIndex">Insertion index in the <paramref name="targetTabControl"/>.</param>
        bool AllowTargetedDrop(object item, TabControl sourceTabControl, int sourceIndex, TabControl targetTabControl, int insertionIndex);

        /// <summary>
        /// Queries if the application allows for the targetless drop (this represents a case when a tab is dropped onto &quot;thin air&quot; as opposed to an accepting tab control).
        /// </summary>
        /// <param name="item">The item associated with the <see cref="T:System.Windows.Controls.TabItem"/> being dropped.</param>
        /// <param name="sourceTabControl">The source tab control.</param>
        /// <param name="sourceIndex">Index of the <paramref name="item"/> in the <paramref name="sourceTabControl"/>.</param>
        /// <param name="dropLocation">The drop location.</param>
        bool AllowTargetlessDrop(object item, TabControl sourceTabControl, int sourceIndex, Point dropLocation);

        /// <summary>
        /// Handles a reorder attempt within the <paramref name="tabControl"/>.
        /// </summary>
        /// <param name="item">The item associated with the <see cref="T:System.Windows.Controls.TabItem"/> being dragged.</param>
        /// <param name="tabControl">The source tab control.</param>
        /// <param name="sourceIndex">Index of the <paramref name="item"/> in the <paramref name="tabControl"/>.</param>
        /// <param name="insertionIndex">Insertion index in the <paramref name="tabControl"/>.</param>
        void HandleReorder(object item, TabControl tabControl, int sourceIndex, int insertionIndex);

        /// <summary>
        /// Handles the targeted drop (this represents a case when a tab is dropped onto another tab control).
        /// </summary>
        /// <param name="item">The item associated with the <see cref="T:System.Windows.Controls.TabItem"/> being dropped.</param>
        /// <param name="sourceTabControl">The source tab control.</param>
        /// <param name="sourceIndex">Index of the <paramref name="item"/> in the <paramref name="sourceTabControl"/>.</param>
        /// <param name="targetTabControl">The target tab control.</param>
        /// <param name="insertionIndex">Insertion index in the <paramref name="targetTabControl"/>.</param>
        void HandleTargetedDrop(object item, TabControl sourceTabControl, int sourceIndex, TabControl targetTabControl, int insertionIndex);

        /// <summary>
        /// Handles a targetless drop (this represents a case when a tab is dropped onto &quot;thin air&quot; as opposed to an accepting tab control).
        /// </summary>
        /// <param name="item">The item associated with the <see cref="T:System.Windows.Controls.TabItem"/> being dropped.</param>
        /// <param name="sourceTabControl">The source tab control.</param>
        /// <param name="sourceIndex">Index of the <paramref name="item"/> in the <paramref name="sourceTabControl"/>.</param>
        /// <param name="dropLocation">The drop location.</param>
        void HandleTargetlessDrop(object item, TabControl sourceTabControl, int sourceIndex, Point dropLocation);

        /// <summary>
        /// Determines whether the specified item is allowed to be dragged from the <paramref name="tabControl"/>.
        /// </summary>
        /// <param name="item">The item associated with the <see cref="T:System.Windows.Controls.TabItem"/> being dragged.</param>
        /// <param name="tabControl">The tab control.</param>
        /// <param name="sourceIndex">Index of the <paramref name="item"/> in the <paramref name="tabControl"/>.</param>
        /// <returns>Boolean.</returns>
        bool IsDragAllowed(object item, TabControl tabControl, int sourceIndex);
    }
}