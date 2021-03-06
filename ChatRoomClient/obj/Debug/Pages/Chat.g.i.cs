﻿#pragma checksum "..\..\..\Pages\Chat.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2636EB6B1B9BF6619650F0D9565096EA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ChatRoom.Pages;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ChatRoom.Pages {
    
    
    /// <summary>
    /// Chat
    /// </summary>
    public partial class Chat : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 21 "..\..\..\Pages\Chat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox chatLogBox;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Pages\Chat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox messageEntry;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\Pages\Chat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button sendButton;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\Pages\Chat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label connectionStatusLabel;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Pages\Chat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label characterCountLabel;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\Pages\Chat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button sendButton_Copy;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ChatRoomClient;component/pages/chat.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Pages\Chat.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 14 "..\..\..\Pages\Chat.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveChatLogClick);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 15 "..\..\..\Pages\Chat.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.ClearChatClick);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 16 "..\..\..\Pages\Chat.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.LeaveRoomClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.chatLogBox = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 5:
            this.messageEntry = ((System.Windows.Controls.TextBox)(target));
            
            #line 27 "..\..\..\Pages\Chat.xaml"
            this.messageEntry.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.messageEntry_TextChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.sendButton = ((System.Windows.Controls.Button)(target));
            
            #line 28 "..\..\..\Pages\Chat.xaml"
            this.sendButton.Click += new System.Windows.RoutedEventHandler(this.sendButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.connectionStatusLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.characterCountLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 9:
            this.sendButton_Copy = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\..\Pages\Chat.xaml"
            this.sendButton_Copy.Click += new System.Windows.RoutedEventHandler(this.sendButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

