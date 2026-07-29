import rhinoscriptsyntax as rs
import math
import random

# ==========================================
# 1. MOTEUR D'ASSEMBLAGE 3D
# ==========================================

def place_part(part_func, plane, scale, material=None):
    """Génère la pièce, la met à l'échelle et l'oriente rigoureusement."""
    ids = part_func() 
    if not ids: return []
    
    # Nettoyer et forcer en liste
    if not isinstance(ids, list): ids = [ids]
    valid_ids = [i for i in ids if i and not isinstance(i, bool)]
    if not valid_ids: return []
    
    # Mise à l'échelle (à l'origine)
    if scale != 1.0:
        rs.ScaleObjects(valid_ids, (0,0,0), (scale, scale, scale))
        
    # Points de référence (Monde) et Points cibles (Plan)
    ref = [(0,0,0), (1,0,0), (0,1,0)]
    tgt = [plane.Origin, rs.PointAdd(plane.Origin, plane.XAxis), rs.PointAdd(plane.Origin, plane.YAxis)]
    
    # Aligner et assigner un matériau si fourni
    for obj in valid_ids:
        rs.OrientObject(obj, ref, tgt)
        
    return valid_ids

def create_hexagon(plane, radius):
    """Génère un polygone hexagonal."""
    pts = [plane.PointAt(radius*math.cos(i*math.pi/3.0), radius*math.sin(i*math.pi/3.0)) for i in range(7)]
    return rs.AddPolyline(pts)

# ==========================================
# 2. BIBLIOTHÈQUE DES PIÈCES MECANIQUES (Ultra Détaillées)
# ==========================================

def p_flanged_pipe():
    return [rs.AddCylinder(rs.WorldXYPlane(), 20.0, 2.5), rs.AddCylinder(rs.WorldXYPlane(), 1.5, 4.5), rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [0,0,18.5]), 1.5, 4.5)]

def p_tooth():
    """Crée une dent pointue (boulon cône)."""
    ids = []
    # Base hexagonale de la dent
    hex_base = create_hexagon(rs.WorldXYPlane(), 0.8)
    if hex_base:
        ext = rs.ExtrudeCurveStraight(hex_base, (0,0,0), (0,0,1.0))
        if ext: ids.append(rs.CapPlanarHoles(ext) or ext)
        rs.DeleteObject(hex_base)
    # Pointe de la dent (cône)
    cone_plane = rs.MovePlane(rs.WorldXYPlane(), [0,0,1.0])
    cone = rs.AddCone(cone_plane, 2.5, 0.8)
    if cone: ids.append(cone)
    return ids

def p_gear_dense():
    """Crée un engrenage dense à rayons."""
    ids = [rs.AddCylinder(rs.WorldXYPlane(), 3.0, 8.0)] # Moyeu épais
    # Spokes (rayons)
    for i in range(5):
        l = rs.AddLine([0,0,1.5], [8*math.cos(i*2*math.pi/5), 8*math.sin(i*2*math.pi/5), 1.5])
        ids += rs.AddPipe(l, 0, 1.2)
        if l: rs.DeleteObject(l)
    # Dents
    for i in range(12):
        tp = rs.MovePlane(rs.WorldXYPlane(), [8.0*math.cos(i*math.pi/6), 8.0*math.sin(i*math.pi/6), 0])
        ids.append(rs.AddCylinder(tp, 3.0, 1.5))
    return ids

def p_piston_detailed():
    """Crée un piston complexe."""
    # Base cylindre annelée
    ids = [rs.AddCylinder(rs.WorldXYPlane(), 15.0, 4.0)]
    for i in range(3):
        ids.append(rs.AddTorus(rs.MovePlane(rs.WorldXYPlane(), [0,0,2+i*4]), 4.0, 0.8))
    # Tige complexe
    rod_plane = rs.MovePlane(rs.WorldXYPlane(), [0,0,15.0])
    ids.append(rs.AddCylinder(rod_plane, 10.0, 1.5))
    # Tête articulée (universel)
    head_plane = rs.MovePlane(rod_plane, [0,0,10.0])
    place_part(p_universal, head_plane, 0.3) # Stacking sécurisé
    return ids

def p_valve_wheel():
    """Crée un volant de vanne détaillé."""
    ids = [rs.AddCylinder(rs.WorldXYPlane(), 3, 2.5), rs.AddTorus(rs.MovePlane(rs.WorldXYPlane(), [0,0,1.5]), 8, 1.2)]
    for i in range(5):
        l = rs.AddLine([0,0,1.5], [8*math.cos(i*2*math.pi/5), 8*math.sin(i*2*math.pi/5), 1.5])
        ids += rs.AddPipe(l, 0, 0.8)
        if l: rs.DeleteObject(l)
    return ids

def p_universal():
    """Crée un joint universel."""
    return [rs.AddCylinder(rs.MovePlane(rs.WorldYZPlane(), [-6,0,3]), 12, 1.8), rs.AddCylinder(rs.MovePlane(rs.WorldZXPlane(), [0,-6,3]), 12, 1.8), rs.AddSphere([0,0,3], 2.5)]

def p_sprocket_heavy():
    """Crée un pignon lourd empilé."""
    # Stacked disks
    ids = [rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [0,0,z]), 2, r) for z, r in [(0,8), (2,6), (4,8)]]
    # Pointy teeth
    for i in range(16):
        cone_plane = rs.MovePlane(rs.WorldXYPlane(), [8*math.cos(i*math.pi/8), 8*math.sin(i*math.pi/8), 0])
        ids.append(rs.AddCylinder(cone_plane, 2.0, 1.5))
    return ids

def p_gauge():
    """Crée un manomètre détaillé."""
    ids = [rs.AddCylinder(rs.WorldXYPlane(), 4, 5), rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [0,0,4]), 0.5, 4.5), rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [0,0,4.5]), 1, 0.5)]
    l = rs.AddLine([0,0,4.6], [3,2,4.6])
    ids += rs.AddPipe(l, 0, 0.3)
    if l: rs.DeleteObject(l)
    return ids

def p_crankshaft_segment():
    """Crée un segment de vilebrequin."""
    # Arbre central
    ids = [rs.AddCylinder(rs.WorldXYPlane(), 20, 2.5)]
    # Bras de vilebrequin stacked
    plane_crank = rs.MovePlane(rs.WorldXYPlane(), [0,0,10.0])
    hex_head = create_hexagon(plane_crank, 6.0)
    if hex_head:
        ext = rs.ExtrudeCurveStraight(hex_head, (0,0,0), (0,0,2.0))
        if ext: ids.append(rs.CapPlanarHoles(ext) or ext)
        rs.DeleteObject(hex_head)
    # Axe décalé
    offset_plane = rs.MovePlane(plane_crank, [3.0, 0, 0])
    ids.append(rs.AddCylinder(offset_plane, 10, 2.0))
    return ids

def p_pulley():
    return [rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [0,0,z]), 2, r) for z, r in [(0,8), (2,6), (4,8)]]

def p_ball_bearing_complex():
    ids = [rs.AddCylinder(rs.WorldXYPlane(), 4, r) for r in [10, 5]]
    ids += [rs.AddSphere([6.5*math.cos(i*math.pi/4), 6.5*math.sin(i*math.pi/4), 2], 1.4) for i in range(8)]
    return ids

# ==========================================
# 3. ANATOMIE EXACTE (Membres & Tête)
# ==========================================

def add_rib(plane, scale, side):
    p0 = plane.Origin
    v1 = rs.VectorAdd(rs.VectorScale(plane.XAxis, 12 * scale * side), rs.VectorScale(plane.YAxis, -4 * scale))
    p1 = rs.PointAdd(p0, v1)
    v2 = rs.VectorAdd(rs.VectorScale(plane.XAxis, 10 * scale * side), rs.VectorScale(plane.YAxis, -20 * scale))
    p2 = rs.PointAdd(p1, v2)
    crv = rs.AddInterpCurve([p0, p1, p2])
    if crv:
        rs.AddPipe(crv, 0, 2.0 * scale) 
        rs.DeleteObject(crv)

def add_leg(plane, scale, side, is_front=False):
    p0 = plane.Origin
    v_out = rs.VectorScale(plane.XAxis, side * 1.5)
    v_down = rs.VectorScale(plane.YAxis, -1.0 if not is_front else -1.5)
    femur_z = rs.VectorAdd(v_out, v_down)
    femur_plane = rs.PlaneFromNormal(p0, femur_z)
    
    place_part(p_universal, femur_plane, scale * 1.5) 
    place_part(p_piston_detailed, femur_plane, scale * 1.5) 
    
    p1 = rs.PointAdd(p0, rs.VectorScale(femur_plane.ZAxis, 25.0 * scale * 1.5))
    v_back = rs.VectorScale(plane.ZAxis, -0.5)
    tibia_z = rs.VectorAdd(v_back, rs.VectorScale(plane.YAxis, -2.0))
    tibia_plane = rs.PlaneFromNormal(p1, tibia_z)
    
    place_part(p_gear_dense, tibia_plane, scale * 1.8) 
    place_part(p_piston_detailed, tibia_plane, scale * 1.2) 
    
    p2 = rs.PointAdd(p1, rs.VectorScale(tibia_plane.ZAxis, 25.0 * scale * 1.2))
    pied_plane = rs.PlaneFromNormal(p2, plane.YAxis) 
    place_part(p_sprocket_heavy, pied_plane, scale * 1.2)
    for spread in [-0.5, 0.0, 0.5]:
        v_claw = rs.VectorAdd(pied_plane.ZAxis, rs.VectorScale(pied_plane.XAxis, spread))
        claw_plane = rs.PlaneFromNormal(p2, v_claw)
        place_part(p_tooth, claw_plane, scale * 1.0)

def add_wing(plane, scale, side):
    p0 = plane.Origin
    v_out = rs.VectorScale(plane.XAxis, side * 2.0)
    v_up = rs.VectorScale(plane.YAxis, 2.0)
    v_back = rs.VectorScale(plane.ZAxis, -0.5)
    arm_z = rs.VectorAdd(rs.VectorAdd(v_out, v_up), v_back)
    arm_plane = rs.PlaneFromNormal(p0, arm_z)
    
    place_part(p_pulley, arm_plane, scale * 2.5) 
    place_part(p_flanged_pipe, arm_plane, scale * 1.5) 
    
    p1 = rs.PointAdd(p0, rs.VectorScale(arm_plane.ZAxis, 20.0 * scale * 1.5))
    v_out2 = rs.VectorScale(plane.XAxis, side * 2.5)
    v_fwd = rs.VectorScale(plane.ZAxis, 1.0)
    forearm_z = rs.VectorAdd(v_out2, v_fwd)
    forearm_plane = rs.PlaneFromNormal(p1, forearm_z)
    
    place_part(p_gear_dense, forearm_plane, scale * 2.2) 
    place_part(p_piston_detailed, forearm_plane, scale * 1.5) 
    
    p2 = rs.PointAdd(p1, rs.VectorScale(forearm_plane.ZAxis, 25.0 * scale * 1.5))
    place_part(p_universal, rs.PlaneFromNormal(p2, forearm_z), scale * 1.8)
    
    for spread in [-1.0, 0.0, 1.0]:
        v_spread = rs.VectorAdd(v_out2, rs.VectorScale(plane.ZAxis, spread))
        v_down = rs.VectorScale(plane.YAxis, -1.0)
        finger_z = rs.VectorAdd(v_spread, v_down)
        finger_plane = rs.PlaneFromNormal(p2, finger_z)
        cyl = rs.AddCylinder(finger_plane, 90 * scale, 1.5 * scale)
        if cyl: rs.CapPlanarHoles(cyl)
        p3 = rs.PointAdd(p2, rs.VectorScale(finger_plane.ZAxis, 90 * scale))
        place_part(p_sprocket_heavy, rs.PlaneFromNormal(p3, finger_z), scale * 1.0)

def add_head_aggressive(plane, scale):
    p0 = plane.Origin
    cranium_parts = [p_gear_dense, p_sprocket_heavy, p_pulley]
    random.shuffle(cranium_parts)
    place_part(random.choice(cranium_parts), plane, scale * 1.5)
    place_part(p_valve_wheel, plane, scale * 1.3)
    
    for side in [1, -1]:
        eye_plane = rs.MovePlane(plane, rs.VectorAdd(rs.VectorScale(plane.XAxis, 6*scale*side), rs.VectorScale(plane.YAxis, 4*scale)))
        eye_plane = rs.RotatePlane(eye_plane, 90*side, eye_plane.YAxis)
        place_part(p_gauge, eye_plane, scale * 1.3)
        eye_plane_rot = rs.RotatePlane(eye_plane, 180, eye_plane.XAxis)
        eye_plane_rot = rs.MovePlane(eye_plane_rot, [0,0,1*scale])
        place_part(p_sprocket_heavy, eye_plane_rot, scale * 0.8)

    for side in [1, -1]:
        v_out = rs.VectorScale(plane.XAxis, side * 3.0)
        v_up = rs.VectorScale(plane.YAxis, 3.0)
        v_back = rs.VectorScale(plane.ZAxis, -0.5)
        horn_z = rs.VectorAdd(rs.VectorAdd(v_out, v_up), v_back)
        horn_base = rs.PointAdd(p0, rs.VectorScale(plane.YAxis, 4.0 * scale))
        horn_plane = rs.PlaneFromNormal(horn_base, horn_z)
        place_part(p_piston_detailed, horn_plane, scale * 1.2)
        p1 = rs.PointAdd(horn_base, rs.VectorScale(horn_plane.ZAxis, 25.0 * scale * 1.2))
        place_part(p_sprocket_heavy, rs.PlaneFromNormal(p1, horn_z), scale * 1.0)

    jaw_len = 40 * scale
    tooth_count = 15 
    tooth_spacing = jaw_len / (tooth_count + 1)
    fwd_down = rs.VectorAdd(rs.VectorScale(plane.ZAxis, 1.0), rs.VectorScale(plane.YAxis, -0.2))
    up_jaw_plane = rs.PlaneFromNormal(rs.PointAdd(p0, rs.VectorScale(plane.ZAxis, 2.0*scale)), fwd_down)
    place_part(p_piston_detailed, up_jaw_plane, scale * 1.5)
    
    for i in range(tooth_count):
        dist = 5.0 * scale + (i + 1) * tooth_spacing
        t_pt = rs.PointAdd(up_jaw_plane.Origin, rs.VectorScale(up_jaw_plane.ZAxis, dist))
        t_pt = rs.PointAdd(t_pt, rs.VectorScale(up_jaw_plane.YAxis, -4.0 * scale))
        t_plane = rs.PlaneFromNormal(t_pt, up_jaw_plane.ZAxis)
        t_plane = rs.RotatePlane(t_plane, 180, t_plane.XAxis) 
        place_part(p_tooth, t_plane, scale * 1.0) 

    low_fwd_down = rs.VectorAdd(rs.VectorScale(plane.ZAxis, 1.0), rs.VectorScale(plane.YAxis, -0.6))
    p_low = rs.PointAdd(p0, rs.VectorScale(plane.YAxis, -4.0 * scale))
    low_jaw_plane = rs.PlaneFromNormal(p_low, low_fwd_down)
    
    place_part(p_piston_detailed, low_jaw_plane, scale * 1.2)
    place_part(p_gear_dense, low_jaw_plane, scale * 1.2)
    
    for i in range(tooth_count - 2): 
        dist = 6.0 * scale + (i + 1) * tooth_spacing
        t_pt = rs.PointAdd(low_jaw_plane.Origin, rs.VectorScale(low_jaw_plane.ZAxis, dist))
        t_pt = rs.PointAdd(t_pt, rs.VectorScale(low_jaw_plane.YAxis, 4.0 * scale))
        t_plane = rs.PlaneFromNormal(t_pt, low_jaw_plane.ZAxis)
        place_part(p_tooth, t_plane, scale * 0.9)

# ==========================================
# 4. GÉNÉRATION DU DRAGON SQUELETTE MÉCANIQUE
# ==========================================

def build_dragon():
    rs.EnableRedraw(False)
    
    # --- COU ALLONGÉ ET TÊTE REPOUSSÉE ---
    points = [
        (-250, -10, 0),    # Pointe de la queue très lointaine
        (-150, 15, -10),   # Ondulation
        (-50, -5, 5),      # Ondulation
        (30, 15, -5),      # Jonction torse
        (70, -10, 10),     # Bassin
        (120, 20, 20),     # Torse
        (160, 5, 10),      # Epaules
        (210, 50, 30),     # Cou allongé et surélevé
        (260, 40, 50)      # Tête repoussée vers l'avant
    ]
    spine = rs.AddInterpCurve(points)
    rs.AddPipe(spine, 0, 2.5) 
    
    divisions = 450 
    points_on_curve = rs.DivideCurve(spine, divisions)
    
    body_parts = [p_gear_dense, p_flanged_pipe, p_crankshaft_segment, p_piston_detailed, p_piston_detailed, p_ball_bearing_complex]
    spine_parts = [p_sprocket_heavy, p_piston_detailed, p_piston_detailed]
    
    # --- RECALCUL PRECIS POUR LA NOUVELLE LONGUEUR ---
    bassin_progres = 0.63  
    epaule_progres = 0.80  
    
    for i, pt in enumerate(points_on_curve):
        t = rs.CurveClosestPoint(spine, pt)
        plane = rs.CurvePerpFrame(spine, t)
        
        progress = float(i) / divisions
        
        if progress < bassin_progres:
            base_scale = 0.3 + (progress / bassin_progres) * 0.7
        elif progress > epaule_progres:
            base_scale = 1.0 - ((progress - epaule_progres) / (1.0 - epaule_progres)) * 0.3
        else:
            p_torso = (progress - bassin_progres) / (epaule_progres - bassin_progres)
            base_scale = 1.0 + (math.sin(p_torso * math.pi) * 0.25)
            
        if i == 0:
            # --- LA BOULE DE COMPOSANTS AU BOUT DE LA QUEUE ---
            place_part(p_ball_bearing_complex, plane, base_scale * 6.0)
            place_part(p_valve_wheel, plane, base_scale * 5.0)
            place_part(p_universal, plane, base_scale * 4.0)
            
        elif i == divisions: 
            add_head_aggressive(plane, base_scale * 3.0)
            
        else:
            if int(divisions * 0.58) < i < int(divisions * 0.78) and i % 3 == 0:
                add_rib(plane, base_scale, 1)
                add_rib(plane, base_scale, -1)
            
            if i % 2 == 0:
                dorsal_plane = rs.PlaneFromNormal(plane.Origin, plane.YAxis)
                dorsal_plane = rs.MovePlane(dorsal_plane, rs.VectorScale(plane.YAxis, 4.0 * base_scale))
                spike_func = random.choice(spine_parts)
                place_part(spike_func, dorsal_plane, base_scale * 1.0)
                
            if i % 3 == 0:
                place_part(p_gear_dense, plane, base_scale * 1.5)
            if i % 4 == 0:
                part_func = random.choice(body_parts)
                place_part(part_func, plane, base_scale * 0.9)

    # Nouveaux index verrouillés mathématiquement
    idx_bassin = int(divisions * bassin_progres) 
    idx_torse = int(divisions * 0.72) 
    idx_epaule = int(divisions * epaule_progres) 
    
    t_bassin = rs.CurveClosestPoint(spine, points_on_curve[idx_bassin])
    plane_bassin = rs.CurvePerpFrame(spine, t_bassin)
    add_leg(plane_bassin, 1.2, 1) 
    add_leg(plane_bassin, 1.2, -1)
                
    t_torse = rs.CurveClosestPoint(spine, points_on_curve[idx_torse])
    plane_torse = rs.CurvePerpFrame(spine, t_torse)
    add_wing(plane_torse, 1.6, 1)
    add_wing(plane_torse, 1.6, -1)
                
    t_epaule = rs.CurveClosestPoint(spine, points_on_curve[idx_epaule])
    plane_epaule = rs.CurvePerpFrame(spine, t_epaule)
    add_leg(plane_epaule, 1.5, 1, is_front=True)
    add_leg(plane_epaule, 1.5, -1, is_front=True)

    rs.EnableRedraw(True)
    print("Dragon Mécanique Final (Boule en bout de queue & Cou allongé) généré avec succès !")

if __name__ == "__main__":
    build_dragon()