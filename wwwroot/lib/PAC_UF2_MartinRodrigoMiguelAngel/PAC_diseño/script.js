
//  Seleccion de elementos en el DOM
const PARTICIPANTES = document.getElementById("participantes")
const PLAY = document.getElementById("play")
const RACE = document.getElementById("race")
const AF = document.querySelector("#race::after")
const START = document.getElementById("start")
const RESTART = document.getElementById("restart")
const TABLE = document.getElementById("table")
const LINE = document.getElementById("race-line")

// Inicialización del array con los participantes
let array_participantes = []

// Botones que desaparecen
START.style.display = "none"
RESTART.style.display = "none"

// Actualizando el array con los valores del select
PARTICIPANTES.addEventListener("change", ()=>{
    array_participantes = []

    if(PARTICIPANTES.value > 0){
        for(let i = 0; i < PARTICIPANTES.value; i++){
            array_participantes.push(i)
        }
        return
    }

})

// Capturando el click en el PLAY
PLAY.addEventListener("click", ()=>{

    if(array_participantes.length < 1) return

    array_participantes.forEach((e)=>{

        // Creando elementos que serán los coches
        RACE.insertAdjacentHTML("beforeend", `
        <img id="car${e + 1}" style="display: block;" src="/img/car${e + 1}.png" width="50px"/>
        `)

    })

    // Mostrando y escondiendo botones
    START.style.display = "inline-block"
    PLAY.style.display = "none"

})

// Array con las posiciones de
let arr_posiciones = []

    $("#start").click(function(){

        array_participantes.forEach((e)=>{

            // Cogiendo el coche en el DOM
            const CAR = document.getElementById(`car${e + 1}`)
                
            // Calculo atleatorio de los segundos
            let random = Math.random() * 10
            let duracion = random * 1000
    
            $(`#car${e + 1}`).animate({marginLeft: "500px", easing: "linear"}, duracion, function(){
                arr_posiciones.push(e + 1)

                // Creando filas de la tabla con las posiciones
                TABLE.insertAdjacentHTML("beforeend", `
                <tr id="fila${e}">
                    <td class="table">${e + 1}</td>
                    <td class="table">${(duracion / 1000).toFixed(3)}s</td>
                </tr> 
                `)

                // Captando la posicion para mostrarla con el coche
                let pos = arr_posiciones.findIndex(car => car == e + 1 )
                CAR.insertAdjacentHTML("beforebegin", `
                <span id="position${e}" style="position: relative; left: 550px"; bottom: 1>${pos + 1}º</span>
                `)
                })
            })

        RESTART.style.display = "inline-block"

        })

        // Captando el click
        RESTART.addEventListener("click", ()=>{

            // Eliminando todo lo generado dinamicamente
            array_participantes.forEach((e)=>{
                
                document.getElementById(`car${e + 1}`).remove()
                document.getElementById(`position${e}`).remove()
                document.getElementById(`fila${e}`).remove()
            })

            // Mostrando el boton de play y ocultando los otros
            START.style.display = "none"
            PLAY.style.display = "inline-block"
            RESTART.style.display = "none"

            // Reseteo tanto de las posiciones como de los participantes
            arr_posiciones = []
            array_participantes = []

        })
