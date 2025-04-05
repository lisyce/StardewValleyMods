<frame border-thickness="16,16,16,22" border={@Mods/StardewUI/Sprites/ControlBorder} background={@Mods/StardewUI/Sprites/ControlBorder} padding="20">
<scrollable layout="750px 500px" peeking="64">
    <lane orientation="vertical">
        <banner text="Mobile Farm Computer" margin="0,0,0,20" />
        <label text={:Location} margin="0,0,0,30" />
        
        <label font="dialogue" text="To-Do" margin="0,0,0,20" />
        <lane orientation="vertical">
            <lane *repeat={:TodoItems}>
                <expander *if={:RenderSubtasks} layout="stretch content" margin="0,0,0,15">
                    <label *outlet="header" text={:TaskName}  />
                    <lane orientation="vertical">
                        <label *repeat={:UncompletedSubtasks} text={:this} margin="90,15,0,0"/>
                    </lane>
                </expander>

                <label *!if={:RenderSubtasks} text={:TaskName} margin="50,0,0,15" />
            </lane>
        </lane>
        <label font="dialogue" text="Completed" margin="0,40,0,20" color="#5c5c5c" />
        <label *repeat={:DoneItems} text={:TaskName} margin="30,0,0,15" color="#5c5c5c" />
    </lane>
</scrollable>
</frame>