
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Protobuild
{
	public partial class GtkProtobuildModuleCreationWidget
	{
		private global::Gtk.HBox mainHBox;
		
		private global::Gtk.EventBox leftBorderEventBox;
		
		private global::Gtk.EventBox projectConfigurationTableEventBox;
		
		private global::Gtk.VBox projectConfigurationVBox;
		
		private global::Gtk.EventBox eventbox1;
		
		private global::Gtk.ProgressBar progressbar1;
		
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.ScrolledWindow GtkScrolledWindow1;
		
		private global::Gtk.TextView protobuildOutputTextArea;
		
		private global::Gtk.EventBox projectConfigurationBottomEventBox;
		
		private global::Gtk.EventBox eventbox2;
		
		private global::Gtk.EventBox projectConfigurationRightBorderEventBox;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Protobuild.GtkProtobuildModuleCreationWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "MonoDevelop.Protobuild.GtkProtobuildModuleCreationWidget";
			// Container child MonoDevelop.Protobuild.GtkProtobuildModuleCreationWidget.Gtk.Container+ContainerChild
			this.mainHBox = new global::Gtk.HBox ();
			this.mainHBox.Name = "mainHBox";
			// Container child mainHBox.Gtk.Box+BoxChild
			this.leftBorderEventBox = new global::Gtk.EventBox ();
			this.leftBorderEventBox.WidthRequest = 30;
			this.leftBorderEventBox.Name = "leftBorderEventBox";
			this.mainHBox.Add (this.leftBorderEventBox);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.mainHBox [this.leftBorderEventBox]));
			w1.Position = 0;
			w1.Expand = false;
			// Container child mainHBox.Gtk.Box+BoxChild
			this.projectConfigurationTableEventBox = new global::Gtk.EventBox ();
			this.projectConfigurationTableEventBox.WidthRequest = 561;
			this.projectConfigurationTableEventBox.Name = "projectConfigurationTableEventBox";
			// Container child projectConfigurationTableEventBox.Gtk.Container+ContainerChild
			this.projectConfigurationVBox = new global::Gtk.VBox ();
			this.projectConfigurationVBox.Name = "projectConfigurationVBox";
			// Container child projectConfigurationVBox.Gtk.Box+BoxChild
			this.eventbox1 = new global::Gtk.EventBox ();
			this.eventbox1.HeightRequest = 30;
			this.eventbox1.Name = "eventbox1";
			this.projectConfigurationVBox.Add (this.eventbox1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.projectConfigurationVBox [this.eventbox1]));
			w2.Position = 0;
			// Container child projectConfigurationVBox.Gtk.Box+BoxChild
			this.progressbar1 = new global::Gtk.ProgressBar ();
			this.progressbar1.Name = "progressbar1";
			this.progressbar1.Fraction = 0.5;
			this.progressbar1.PulseStep = 0.35;
			this.projectConfigurationVBox.Add (this.progressbar1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.projectConfigurationVBox [this.progressbar1]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child projectConfigurationVBox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.HeightRequest = 500;
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.protobuildOutputTextArea = new global::Gtk.TextView ();
			this.protobuildOutputTextArea.CanFocus = true;
			this.protobuildOutputTextArea.Name = "protobuildOutputTextArea";
			this.GtkScrolledWindow1.Add (this.protobuildOutputTextArea);
			this.vbox2.Add (this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow1]));
			w5.Position = 0;
			this.projectConfigurationVBox.Add (this.vbox2);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.projectConfigurationVBox [this.vbox2]));
			w6.Position = 2;
			// Container child projectConfigurationVBox.Gtk.Box+BoxChild
			this.projectConfigurationBottomEventBox = new global::Gtk.EventBox ();
			this.projectConfigurationBottomEventBox.HeightRequest = 30;
			this.projectConfigurationBottomEventBox.Name = "projectConfigurationBottomEventBox";
			// Container child projectConfigurationBottomEventBox.Gtk.Container+ContainerChild
			this.eventbox2 = new global::Gtk.EventBox ();
			this.eventbox2.Name = "eventbox2";
			this.projectConfigurationBottomEventBox.Add (this.eventbox2);
			this.projectConfigurationVBox.Add (this.projectConfigurationBottomEventBox);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.projectConfigurationVBox [this.projectConfigurationBottomEventBox]));
			w8.Position = 3;
			this.projectConfigurationTableEventBox.Add (this.projectConfigurationVBox);
			this.mainHBox.Add (this.projectConfigurationTableEventBox);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.mainHBox [this.projectConfigurationTableEventBox]));
			w10.Position = 1;
			// Container child mainHBox.Gtk.Box+BoxChild
			this.projectConfigurationRightBorderEventBox = new global::Gtk.EventBox ();
			this.projectConfigurationRightBorderEventBox.WidthRequest = 30;
			this.projectConfigurationRightBorderEventBox.Name = "projectConfigurationRightBorderEventBox";
			this.mainHBox.Add (this.projectConfigurationRightBorderEventBox);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.mainHBox [this.projectConfigurationRightBorderEventBox]));
			w11.Position = 2;
			w11.Expand = false;
			this.Add (this.mainHBox);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
