alias mToast_dll return $qt($scriptdirmToast.dll)
alias mToast_debug return $false
alias mirc.png return $scriptdirmIRC.png

;;
; Register com and install shortcut
;;
on *:start:{
  mToast.Initialize
}


;; /mToast.Initialize
;;
;;    Registers Com server and installs shortcut
;;    
alias mToast.Initialize {
  return $dll($mToast_dll,Initialize,)
}


;; $mToast.ShowToast([@Title], @Text, [@Image])
;;
;;    Displays a basic toast notification
;;
;;    Returns - String
;;        Toast Tag
;;
;;    @Title - String - Optional
;;        Title of the toast
;;
;;    @Text - String - Required
;;        Body content of the toast
;;
;;    @Image - String - Optional
;;        Absolute file path of logo
;;
alias mToast.ShowToast {
  var %title = $json.encode($1)
  var %text = $json.encode($$2)
  var %image = $json.encode($3)

  var %request = {"Title":" $+ %title $+ ","Body":" $+ %text $+ ","LogoFilePath":" $+ %image $+ "}

  return $mToast.ShowToastJson(%request)
}


;; $mToast.ShowToastXml(@Xml, [@Tag], [@Group])
;;
;;    Displays a custom toast notification
;;
;;    Returns - String
;;        Toast Tag
;;
;;    @Xml - String - Required
;;        XML used to create toast
;;        Design: https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/adaptive-interactive-toasts
;;        Schema: https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/toast-xml-schema
;;
;;    @Tag - String - Optional
;;        Unique identifier for notification
;;
;;    @Group - String - Optional
;;        String to group notifications by
;;
alias mToast.ShowToastXml {
  var %xml = $$1, %tag = $2, %group = $iif($3,$3,mToast)

  if (%tag != $null) dll $mToast_dll SetNextTag %tag
  if (%group != $null) dll $mToast_dll SetNextGroup %group

  var %tag = $dll($mToast_dll,ShowToastXml,%xml)

  if ($mToast_debug) echo 16 -sag Toast created tag = %tag

  return %tag
}


;; $mToast.ShowToastJson(@Json)
;;
;;    Displays a custom toast notification
;;
;;    Returns - String
;;        Toast Tag
;;
;;    @Json - String - Required
;;        Available data members:
;;            Title - String - Optional
;;            Body  - String - Required
;;            BodyList - List<String> - Optional
;;            LogoFilePath - String - Optional
;;            Audio - Number - Optional
;;                Range: 0 (default) - 25 (silent)
;;                See: https://docs.microsoft.com/en-us/uwp/schemas/tiles/toastschema/element-audio
;;            Xml - String - Optional
;;                Other elements are ignored when using this property
;;            Group - String - Optional
;;            SuppressPopup - Bool - Optional
;;            Tag - String - Optional
;;
alias mToast.ShowToastJson {
  var %json = $$1

  var %tag = $dll($mToast_dll,ShowToastJson,%json)

  if ($mToast_debug) echo 7 -sag Toast created tag = %tag

  return %tag
}


;; /mToast.Clear
;;
;;    Clears all notifications created by this application
;;
alias mToast.Clear {
  noop $dll($mToast_dll,Clear,$null)
}


;; /mToast.Remove @Tag @Group
;;
;;    Removes a single notification
;;
;;    @Tag - String - Required
;;        Tag returned from toast creation
;;
;;    @Group - String - Optional
;;        Group given during toast creation
;;
alias mToast.Remove {
  var %tag = $$1, %group = $iif($2,$2,mToast)

  dll $mToast_dll SetNextGroup %group
  noop $dll($mToast_dll,Remove,%tag)
}


;; /mToast.RemoveGroup @Group
;;
;;    Clears all notifications with specified group
;;
;;    @Group - String - Required
;;        Group given during toast creation
;;
alias mToast.RemoveGroup {
  noop $dll($mToast_dll,RemoveGroup,$$1)
}


;; /mToast.SetOnActivatedCallback @Alias
;;
;;    Registers new callback for OnActivated event
;;
;;    @Alias - String - Required
;;        The user-defined alias
;;
alias mToast.SetOnActivatedCallback {
  dll $mToast_dll SetOnActivatedCallback $$1
}


;; /mToast.SetOnCompleteCallback @Alias
;;
;;    Registers new callback for OnComplete event
;;
;;    @Alias - String - Required
;;        The user-defined alias
;;
alias mToast.SetOnCompleteCallback {
  dll $mToast_dll SetOnCompleteCallback $$1
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
  var %args = $$1, %data = $$2

  if ($mToast_debug) echo -sag mToast.OnActivated Args = %args
  if ($mToast_debug) echo -sag mToast.OnActivated Data = %data

  if ($regex(%args,/callback=([^&]+)/i)) {
    var %eval = $ $+ $regml(1) $+ ( % $+ args $chr(44) % $+ data )
    noop $(%eval,2)
  }
}

;; $mToast.OnComplete(@Tag, @Result)
;;
;;    Default callback for OnComplete event
;;
;;    @Tag - String
;;        Tag assigned at toast creation
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
  var %tag = $$1, %result = $$2

  if ($mToast_debug) echo -sag mToast.OnComplete Tag = %tag :: Result = %result

  .signal mToast.OnComplete %result %tag
}

;;
;; Utility functions
;;

alias xml.encode {
  var %ret = $regsubex($strip($1-),/[\x00-\x19]/ugi,)
  var %ret = $replacex(%ret,",&quot;,<,&gt;,&,&amp;)
  return %ret
}

alias -l json.encode {
  var %ret = $regsubex($strip($1-),/[\x00-\x19]/sugi,)
  return $replacex(%ret,",\",\,\\)
}

alias json.unescape {
  return $utfdecode($regsubex($1-,/\\u(d[89a-f][0-9a-f]{2})\\u(d[0-9a-f]{3})|(?:\\(?:u(....)|(.)))/Figu,$escape.map(\1 \3,\2,\4)))
}

alias -l escape.map {
  if ($1) return $chr($base($1,16,10)) $+ $chr($base($2,16,10))
  if ($3 !isalnum) return $3
  if ($2 !isalnum) return $2
  return $chr(160)
}

;;
;; Module for private message toasts
;;

#mToast.Module.PM on

; Toasts for private messages
on *:text:*:?:{
  if ($appactive) return
  if ($halted) return
  if ($msgstamp) && ($calc($ctime - $msgstamp) > 10) { return }

  var %logo = $iif($user.icon($nick),$v1,$mirc.png)

  var %launch.args = callback=mToast.pm.callback&amp;action=launch&amp;cid= $+ $cid $+ &amp;nick= $+ $nick
  var %reply.args = callback=mToast.pm.callback&amp;action=reply&amp;cid= $+ $cid $+ &amp;nick= $+ $nick
  var %view.args = callback=mToast.pm.callback&amp;action=view&amp;cid= $+ $cid $+ &amp;nick= $+ $nick
  var %dismiss.args = callback=mToast.pm.callback&amp;action=dismiss&amp;nick= $+ $nick
  var %header.args = %view.args

  var %xml = <toast launch=" $+ %launch.args $+ "><visual><binding template="ToastGeneric"><text> $+ $nick $+ </text><text> $+ $xml.encode($1-) $+ </text><image src=" $+ %logo $+ " placement="appLogoOverride" hint-crop="circle" /></binding></visual><actions><input id="tbReply" type="text" placeHolderContent="Type a response" /><action content="Reply" arguments=" $+ %reply.args $+ " /><action content="View" arguments=" $+ %view.args $+ " /></actions></toast>

  var %tag = $mToast.ShowToastXml(%xml,,$nick)

  hadd -mu60 mToast %tag pm $cid $nick
}

; Callback for private message toast
alias mToast.pm.callback {
  var %args = $1, %data = $2

  if ($regex(%args,/action=([^&]+)&cid=([^&]+)&nick=([^&]+)/)) {
    var %action = $regml(1)
    var %cid = $regml(2)
    var %nick = $regml(3)

    if (%action == reply) && ($regex(%data,/^{"tbReply":"(.+)"}$/)) {
      var %reply = $json.unescape($regml(1))

      scid %cid
      msg %nick %reply
      flash -c
      window -g0 %nick
    }
    elseif (%action == view) {
      scid %cid
      query %nick
      showmirc -s
    }
    elseif (%action == launch) {
      flash -c
      mToast.RemoveGroup %nick
    }
  }
}

on *:signal:mToast.OnComplete:{
  var %result = $$1, %tag = $$2-

  if ($mToast_debug) echo 3 -sag Signal mToast.OnComplete received result = %result : tag = %tag

  if ($hget(mToast)) && ($hget(mToast,%tag)) { 
    tokenize 32 $v1
    var %action = $1, %cid = $2, %nick = $3
    if (%result == Activated) || (%result == UserCanceled) {
      scid %cid
      flash -c
      if ($window(%nick)) window -g0 %nick
      mToast.RemoveGroup %nick
    }
  }
}

on *:active:?:{
  mToast.RemoveGroup $active
}

; User icons
alias -l user.icon {
  var %file = $+($scriptdir,user.icons\,$nick,.jpg)

  if ($isfile(%file)) return %file
}
#mToast.Module.PM end
