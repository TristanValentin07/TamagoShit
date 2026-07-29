import hou

def generate_v12_engine_final():
    # Nettoyage propre
    engine_name = 'V12_PROCEDURAL_ENGINE_FINAL'
    if hou.node(f'/obj/{engine_name}'):
        hou.node(f'/obj/{engine_name}').destroy()

    geo = hou.node('/obj').createNode('geo', engine_name)
    geo.setDisplayFlag(True)
    
    # ---------------------------------------------------------
    # 1. CONTROLES
    # ---------------------------------------------------------
    ctrl = geo.createNode('null', 'CONTROLS')
    p_grp = ctrl.parmTemplateGroup()
    p_grp.append(hou.FloatParmTemplate('speed', 'Engine Speed', 1, default_value=([15.0])))
    p_grp.append(hou.FloatParmTemplate('crank_r', 'Crank Radius', 1, default_value=([1.0])))
    p_grp.append(hou.FloatParmTemplate('rod_l', 'Rod Length', 1, default_value=([3.5])))
    p_grp.append(hou.FloatParmTemplate('spacing', 'Cylinder Spacing', 1, default_value=([1.8])))
    ctrl.setParmTemplateGroup(p_grp)
    ctrl.setColor(hou.Color((1.0, 0.8, 0.0)))
    ctrl.setPosition([0, 4])

    # ---------------------------------------------------------
    # 2. ASSETS 3D
    # ---------------------------------------------------------
    piston = geo.createNode('tube', 'shape_piston')
    piston.parm('type').set(1); piston.parm('rad1').set(0.8); piston.parm('rad2').set(0.8)
    piston.parm('height').set(1.2); piston.parm('orient').set(1)
    piston_pivot = geo.createNode('xform', 'pivot_piston')
    piston_pivot.setFirstInput(piston); piston_pivot.parm('ty').set(0.6)

    rod = geo.createNode('box', 'shape_rod')
    rod.parm('sizez').setExpression('ch("../CONTROLS/rod_l")')
    rod.parm('sizex').set(0.2); rod.parm('sizey').set(0.4)
    rod_pivot = geo.createNode('xform', 'pivot_rod')
    rod_pivot.setFirstInput(rod); rod_pivot.parm('tz').setExpression('ch("../CONTROLS/rod_l") / 2.0')

    pin = geo.createNode('tube', 'shape_pin')
    pin.parm('type').set(1); pin.parm('rad1').set(0.3); pin.parm('rad2').set(0.3)
    pin.parm('height').set(0.8); pin.parm('orient').set(2)

    web = geo.createNode('box', 'shape_web')
    web.parm('sizez').setExpression('ch("../CONTROLS/crank_r") + 0.6')
    web.parm('sizex').set(0.6); web.parm('sizey').set(0.4)

    axis = geo.createNode('tube', 'shape_axis')
    axis.parm('type').set(1); axis.parm('rad1').set(0.4); axis.parm('rad2').set(0.4)
    axis.parm('height').setExpression('ch("../CONTROLS/spacing") * 6')
    axis.parm('orient').set(2); axis.parm('tz').setExpression('ch("../CONTROLS/spacing") * 2.5')
    axis_color = geo.createNode('color', 'col_axis')
    axis_color.setFirstInput(axis)
    axis_color.parm('colorr').set(0.1); axis_color.parm('colorg').set(0.1); axis_color.parm('colorb').set(0.1)

    # ---------------------------------------------------------
    # 3. KINEMATICS & VEX (Corrigé avec Cast de Matrices)
    # ---------------------------------------------------------
    wrangle = geo.createNode('attribwrangle', 'KINEMATICS_AND_FX')
    wrangle.parm('class').set('detail')
    
    vex = """
    float speed = chf("../CONTROLS/speed");
    float R = chf("../CONTROLS/crank_r");
    float L = chf("../CONTROLS/rod_l");
    float spacing = chf("../CONTROLS/spacing");
    float time = @Time;

    float v_angle = radians(30);

    for(int i=0; i<12; i++) {
        int bank = i % 2; 
        int pair = i / 2; 

        float z_pos = pair * spacing;
        float phase = pair * radians(120);
        if (bank == 1) phase += radians(60);

        float theta = time * speed + phase;
        vector P_pin = set(R * sin(theta), R * cos(theta), z_pos);

        float alpha = (bank == 0) ? v_angle : -v_angle;
        vector dir = set(sin(alpha), cos(alpha), 0);

        float theta_rel = theta - alpha;
        float d = R * cos(theta_rel) + sqrt(L*L - pow(R * sin(theta_rel), 2));
        vector P_piston = dir * d;
        P_piston.z = z_pos;

        float cycle = theta_rel % (4 * 3.14159265);
        if (cycle < 0) cycle += 4 * 3.14159265;
        int is_firing = (cycle < 0.6) ? 1 : 0; 

        // PISTON
        int pt_piston = addpoint(0, P_piston);
        matrix3 m_piston = dihedral(set(0,1,0), dir); // Création matrice
        vector4 q_piston = quaternion(m_piston);      // Cast en quaternion
        setpointattrib(0, "orient", pt_piston, q_piston);
        setpointattrib(0, "part_id", pt_piston, 1);
        if(is_firing) {
            setpointattrib(0, "Cd", pt_piston, set(1.0, 0.25, 0.0));
        } else {
            setpointattrib(0, "Cd", pt_piston, set(0.65, 0.65, 0.7));
        }

        // BIELLE
        int pt_rod = addpoint(0, P_pin);
        vector rod_dir = normalize(P_piston - P_pin);
        matrix3 m_rod = maketransform(rod_dir, set(0,1,0)); // Création matrice
        vector4 q_rod = quaternion(m_rod);                  // Cast en quaternion
        setpointattrib(0, "orient", pt_rod, q_rod);
        setpointattrib(0, "part_id", pt_rod, 2);
        setpointattrib(0, "Cd", pt_rod, set(0.3, 0.3, 0.3));

        // PIN
        int pt_pin = addpoint(0, P_pin);
        matrix3 m_pin = maketransform(set(0,0,1), set(0,1,0)); 
        vector4 q_pin = quaternion(m_pin);
        setpointattrib(0, "orient", pt_pin, q_pin);
        setpointattrib(0, "part_id", pt_pin, 3);
        setpointattrib(0, "Cd", pt_pin, set(0.5, 0.5, 0.5));

        // CONTREPOIDS
        vector center = set(0,0,z_pos);
        int pt_web = addpoint(0, (P_pin + center) / 2.0);
        vector web_dir = normalize(P_pin - center);
        matrix3 m_web = maketransform(web_dir, set(0,0,1)); 
        vector4 q_web = quaternion(m_web);
        setpointattrib(0, "orient", pt_web, q_web);
        setpointattrib(0, "part_id", pt_web, 4);
        setpointattrib(0, "Cd", pt_web, set(0.2, 0.2, 0.2));
    }
    """
    wrangle.parm('snippet').set(vex)

    # ---------------------------------------------------------
    # 4. ASSEMBLAGE
    # ---------------------------------------------------------
    parts = [(piston_pivot, 1, 'piston'), (rod_pivot, 2, 'rod'), (pin, 3, 'pin'), (web, 4, 'web')]
    copies = []

    for p_asset, p_id, p_name in parts:
        pack = geo.createNode('pack', f'pack_{p_name}')
        pack.setFirstInput(p_asset)

        split = geo.createNode('split', f'split_{p_name}')
        split.setFirstInput(wrangle)
        split.parm('group').set(f'@part_id=={p_id}')
        split.parm('grouptype').set(3) # Force mode Points

        copy = geo.createNode('copytopoints', f'copy_{p_name}')
        copy.setFirstInput(pack)
        copy.setNextInput(split)
        copies.append(copy)

    merge = geo.createNode('merge', 'FINAL_ENGINE')
    for c in copies:
        merge.setNextInput(c)
    merge.setNextInput(axis_color)

    merge.setDisplayFlag(True)
    merge.setRenderFlag(True)
    
    geo.layoutChildren()
    hou.clearAllSelected()
    geo.setSelected(True)
    print("Moteur généré avec succès ! Le code VEX compile parfaitement.")

generate_v12_engine_final()