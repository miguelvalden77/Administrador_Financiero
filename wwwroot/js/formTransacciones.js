function InitFormTransacciones (){
    $("#TipoOperacionId").change(async function(){
        const valorSeleccionado = $(this).val()

        const respuesta = await fetch(urlAction, {
            method: "POST",
            body: valorSeleccionado,
            headers: {
                "Content-Type": "application/json"
            }
        })

        const json = await respuesta.json()
        
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`)
        console.log(opciones)
        $("#CategoriaId").html(opciones)
    })
}