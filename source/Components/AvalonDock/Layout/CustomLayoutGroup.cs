using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Markup;


/*
*┌─────────────────────────────────┐
*│　描    述：CustomLayoutGroup
*│　作    者：lispo
*│  联系方式: lis.postbox@gmail.com
*│　版    本：v 0.1
*│　创建时间：2023/7/8 14:56:53
*│  修改日志:
*│  版权说明: www.flowhub.cn Inc. All right reserved.
*└─────────────────────────────────┘
*/
namespace AvalonDock.Layout {

	[ContentProperty(nameof(Children))]
	[Serializable]
	public class CustomLayoutGroup :LayoutPositionableGroup<LayoutAnchorable>, ILayoutPositionableElement {
		protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

		#region Constructors

		/// <summary>Class constructor</summary>
		public CustomLayoutGroup() {
		}

		/// <summary>Class constructor from <see cref="LayoutAnchorable"/> which will be added into its children collection.</summary>
		public CustomLayoutGroup(LayoutAnchorable anchorable) {
			Children.Add(anchorable);
		}

		#endregion Constructors
	}
}
