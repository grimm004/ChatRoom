   M S B A M L   `   `   `  ����I  EChatRoomClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:ChatRoom.PagesChatRoom.Pages  5          ChatRoom.Pages.Chat   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .localclr-namespace:ChatRoom.Pages     5        N� Title$
  Chat��5      .��5
      � .+�5   
   ]� .+�5      o� $	��Top=�6   $��16��6(   .@�5      m� $��_File��6   .@�5      m� -   5        $��_Save Chat Log��5      67   m� 5      -   5        $��_Clear��5      6/   m� 5      -   5        $��_Leave Room��5      64   5      5      �� 5      6   5      �� 5   
   -   5         ��Name$ 
chatLogBox��5      $��10,26,10,50q�6*   {�.  �� 
IsReadOnly . $
��Auto=�5      $
��Auto=�5      ��. . �5      /�  /� 
LineHeight$ 2��6   5      �� 5   
   -   5         ��Name$ messageEntry��5      $��23��6(   $��	9,0,90,21q�64    �� TextWrapping$
 Wrap=�6G   $��Bottom=�6[    � ToolTip$ Message to send.��6�   �� 5   
   -   5         ��Name$ 
sendButton��5      $
��Send��6%   $��Right=�64   $��	0,0,10,21q�6P   $��Bottom=�6c   $��75��6~   $��23��6�   $ Send message.��6�   �� 5   
   -   5         ��Name$ connectionStatusLabel��5      $��Disconnected��6/   $��Right=�6F   $��Right=�6b   $	��Top=�6�   $��Bottom=�6�   	 J� RenderTransformOrigin$	 1.079,0.038&�6�   $	��174��6�   $��26��6�   �� 5   
   -   5        $ characterCountLabel��5      $��0/1024��6-   $��Bottom=�6>   $��23��6Y   $��68��6e   $
��Left=�6p   $4 .The maximum message length is 1024 characters.��6�   $��Right=�6�   $	��Top=�6�   $��Expanded&�6  $��Italic$�6  {�.  $��0,0,0,3q�6B  �� 5   
   -	   5        $ sendButton_Copy��5      $
��Send��6*   $��Right=�69   $��	0,0,10,21q�6U   $��Bottom=�6h   $��75��6�   $��23��6�   $ Send message.��6�   5       5!      