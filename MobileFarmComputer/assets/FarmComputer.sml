<frame border-thickness="16,16,16,22" border={@Mods/StardewUI/Sprites/ControlBorder} background={@Mods/StardewUI/Sprites/ControlBorder} padding="20">
<scrollable layout="750px 500px" peeking="64">
    <lane orientation="vertical">
        <banner text="Mobile Farm Computer" margin="0,0,0,20" />
        <label text={:Location} margin="0,0,0,30" />
        
        <label font="dialogue" text="To-Do" margin="0,0,0,20" />
        <label *repeat={:TodoItems} text={:TaskName} margin="30,0,0,15" />
        
        <label font="dialogue" text="Completed" margin="0,40,0,20" color="#5c5c5c" />
        <label *repeat={:DoneItems} text={:TaskName} margin="30,0,0,15" color="#5c5c5c" />
    </lane>
</scrollable>
</frame>