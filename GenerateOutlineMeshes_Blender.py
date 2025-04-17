import bpy

generated_meshes = []

#additional checks can be added in here. You could use a custom property to filter
#meshes that should have outlines applied
for original_obj in bpy.context.selected_objects:
    if original_obj.type == "MESH":

        #duplicate the selected mesh object
        outline_obj = original_obj.copy()
        outline_obj.data = original_obj.data.copy()

        #append _Outline to the object name
        outline_obj.name = (original_obj.name +"_OUTLINE_")

        #remove geometry nodes modifiers - replace this with whatever
        #check you want to catch Autosmooth
        modifiers_to_remove = []    
        for modifier in outline_obj.modifiers:
            if modifier.type == "NODES":
                modifiers_to_remove.append(modifier)

        for i in range(len(modifiers_to_remove)):
            outline_obj.modifiers.remove(modifiers_to_remove[-1])

        outline_mesh = outline_obj.data
        #ensures normals are reset to their default values
        outline_mesh.normals_split_custom_set([(0, 0, 0) for l in outline_mesh.loops])
        
        #remove sharp edges
        for edge in outline_mesh.edges:
            edge.use_edge_sharp = False
        
        #smooth all faces
        for face in outline_mesh.polygons:
            face.use_smooth = True 

        #store meshes for cleanup post export
        generated_meshes.append(outline_obj)

        #allow the new object to show in the scene - also required for it to be selectable in an exporter
        bpy.context.collection.objects.link(outline_obj)