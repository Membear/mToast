alias -l mToast_dll return $qt($scriptdirmToast.dll)
alias -l mToast_debug return $false
alias -l mirc.png return $scriptdirmIRC.png

; Example toast
alias mtoast.test {
  var %xml = <toast launch="action=viewConversation&amp;conversationId=5"><visual><binding template="ToastGeneric"><text>Membear sent you a picture</text><text>Check this out, The Enchantments!</text><image src=" $+ $mirc.png $+ " /><image src=" $+ $mirc.png $+ " placement="appLogoOverride" /></binding></visual><actions><input id="tbReply" type="text" placeHolderContent="Type a response" /><action content="Reply" arguments="action=reply&amp;conversationId=5" /><action content="Like" arguments="action=like&amp;conversationId=5" /><action content="View" arguments="action=viewImage&amp;imageUrl=https%3A%2F%2Fpicsum.photos%2F364%2F202%3Fimage%3D883" /></actions></toast>

  noop $mToast.ShowCustomToast(%xml)
}

; Toasts for private messages
on *:text:*:?:{
  if ($appactive) return

  var %logo = $iif($user.icon($nick),$v1,$mirc.png)

  var %launch.args = callback=mToast.pm.callback&amp;action=launch&amp;cid= $+ $cid $+ &amp;nick= $+ $nick
  var %reply.args = callback=mToast.pm.callback&amp;action=reply&amp;cid= $+ $cid $+ &amp;nick= $+ $nick
  var %view.args = callback=mToast.pm.callback&amp;action=view&amp;cid= $+ $cid $+ &amp;nick= $+ $nick
  var %header.args = %view.args

  var %xml = <toast launch=" $+ %launch.args $+ "><header id=" $+ $nick $+ " title=" $+ $nick $+ " arguments=" $+ %header.args $+ "/><visual><binding template="ToastGeneric"><text> $+ $1- $+ </text><image src=" $+ %logo $+ " placement="appLogoOverride" hint-crop="circle" /></binding></visual><actions><input id="tbReply" type="text" placeHolderContent="Type a response" /><action content="Reply" arguments=" $+ %reply.args $+ " /><action content="View" arguments=" $+ %view.args $+ " /></actions></toast>

  var %id = $mToast.ShowCustomToast(%xml)
}

; Callback for private message toast
alias mToast.pm.callback {
  var %args = $1, %data = $2

  if ($regex(%args,/action=([^&]+)&cid=([^&]+)&nick=([^&]+)/)) {
    var %action = $regml(1)
    var %cid = $regml(2)
    var %nick = $regml(3)

    if (%action == reply) && ($regex(%data,/^{"tbReply":"(.+)"}$/)) {
      var %reply = $regml(1)

      scid %cid
      msg %nick %reply
    }
    else if (%action == view) {
      scid %cid
      query %nick
      showmirc -s
    }
  }
}

; User icons
alias -l user.icon {
  if ($1 == $me) return $scriptdiruser.icons\clown.jpg
}


;; $mToast.ShowToast(@Line1, @Line2, [@Image])
;;
;;    Displays a basic toast notification
;;
;;    Returns - Number
;;        Toast ID
;;
;;    @Line1 - String - Required
;;        First line in toast
;;
;;    @Line2 - String - Required
;;        Second line in toast
;;
;;    @Image - String - Optionals
;;        Absolute file path of image in toast
;;
alias mToast.ShowToast {
  var %line1 = $$1
  var %line2 = $$2
  var %image = $3

  noop $dll($mToast_dll,SetLine1,%line1)
  noop $dll($mToast_dll,SetLine2,%line2)

  if (%image) { noop $dll($mToast_dll,SetLogoPath,%image) }

  var %id = $dll($mToast_dll,ShowToastAsync,)

  if ($mToast_debug) echo -sag Toast created id = %id

  return %id
}


;; /mToast.ShowCustomToast @Xml
;;
;;    Displays a custom toast notification
;;
;;    Returns - Integer
;;        Toast ID
;;
;;    @Xml - String - Required
;;        XML used to create toast
;;        Design: https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/adaptive-interactive-toasts
;;        Schema: https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/toast-xml-schema
;;
alias mToast.ShowCustomToast {
  var %xml = $$1-

  var %id = $dll($mToast_dll,ShowCustomToastAsync,%xml)
  if ($mToast_debug) echo -sag Toast created id = %id

  return %id
}


;; /mToast.SetOnActivatedCallback @Alias
;;
;;    Registers new callback for OnActivated event
;;
;;    @Alias - String - Required
;;        The user-defined alias
;;
alias mToast.SetOnActivatedCallback {
  var %callback = $$1

  dll $mToast_dll SetOnActivatedCallback %callback
}


;; /mToast.SetOnCompleteCallback @Alias
;;
;;    Registers new callback for OnComplete event
;;
;;    @Alias - String - Required
;;        The user-defined alias
;;
alias mToast.SetOnCompleteCallback {
  var %callback = $$1

  dll $mToast_dll SetOnCompleteCallback %callback
}

;; $mToast.OnActivated(@Args, @Data)
;;
;;    Default callback for OnActivated event
;;    Routes to a custom callback specified in @Args as callback=<alias>
;;
;;    @Args - String
;;        Arguments defined on selected action
;;
;;    @Data - JSON
;;        User input data
;;
alias mToast.OnActivated {
  var %args = $1, %data = $2

  if ($mToast_debug) echo -sag mToast.OnActivated Args = %args
  if ($mToast_debug) echo -sag mToast.OnActivated Data = %data

  if ($regex(%args,/callback=([^&]+)/i)) {
    var %eval = $ $+ $regml(1) $+ ( % $+ args $chr(44) % $+ data )
    noop $(%eval,2)
  }
}

;; $mToast.OnComplete(@Id, @Result)
;;
;;    Default callback for OnComplete event
;;
;;    @Id - Integer
;;        Toast ID assigned at toast creation
;;
;;    @Result - String
;;        -Unavailable
;;        -Invalid
;;        -Activated
;;        -ApplicationHidden
;;        -UserCanceled
;;        -TimedOut
;;        -Failed
;;
alias mToast.OnComplete {
  var %id = $1
  var %result = $2

  if ($mToast_debug) echo -sag mToast.OnComplete Id = %id :: Result = %result
}
