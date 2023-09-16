open System.IO
open System

let wait (waitTime: TimeSpan) =
    task {
        printfn $"Sleeping for {waitTime}"
        Async.Sleep(int32 waitTime.TotalMilliseconds) |> ignore
    }

[<EntryPoint>]
let main argv =
    let random = new Random(int32 DateTime.Now.Ticks)

    let w =
        async {
            for i = 0 to 10 do
                let sleepTime = TimeSpan.FromSeconds(random.Next 10)

                do! wait sleepTime |> Async.AwaitTask
        }

    //let workflow =
    //    task {
    //        let! lines = File.ReadAllLinesAsync("c:/!_DATA/!_DEL/appsettings.Local.json")

    //        let lines = lines |> String.concat "\n"

    //        printfn "%s" lines
    //    }

    w |> Async.RunSynchronously

    Console.ReadLine() |> ignore

    0
