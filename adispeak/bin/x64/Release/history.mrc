alias f6 {
  if ($active == @history) {
    window -c @history
    halt
    } else {
    history
  }
}

alias hspeak {
  sout $1-
}
alias history {
  set %h.a $active
  set %h.t $strip($line($active, 0))
  set %h.l $strip($line($active, 0))
  window -ax @history
  hspeak $strip($line(%h.a, %h.l))
}
on *:keydown:@history:32:{
  hspeak $strip($line(%h.a, %h.l))
}
on *:keydown:@history:27:{
  /window -c @history
}
;k
on *:keydown:@history:38:{
  dec %h.l
  if (%h.l < 1) set %h.l 1
  hspeak $strip($line(%h.a, %h.l))
}
;down
on *:keydown:@history:40:{
  inc %h.l
  if (%h.l > %h.t) set %h.l %h.t
  hspeak $strip($line(%h.a, %h.l))
}
on *:keydown:@history:67:{
  /clipboard $line(%h.a, %h.l)
  hspeak copied
}
