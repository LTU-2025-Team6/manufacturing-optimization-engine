using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Common.Models.Enums;
using Spectre.Console;
using System.Net.Http.Json;
using System.Text.Json;

// Configuration
var apiUrl = Environment.GetEnvironmentVariable("GATEWAY_API_URL") ?? "http://localhost:5000";
var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };

// Welcome screen
AnsiConsole.Clear();
AnsiConsole.Write(new FigletText("Manufacturing").Centered().Color(Color.Blue));
AnsiConsole.Write(new FigletText("Optimization").Centered().Color(Color.Green));
AnsiConsole.WriteLine();

// Select role
var role = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[yellow]Who are you?[/]")
        .AddChoices("Customer", "Provider"));

AnsiConsole.Clear();

if (role == "Customer")
{
    await RunCustomerMode();
}
else
{
    await RunProviderMode();
}

async Task RunCustomerMode()
{
    AnsiConsole.Write(new Rule("[green]Customer Mode[/]").RuleStyle("green"));
    AnsiConsole.WriteLine();

    while (true)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]What do you want to do?[/]")
                .AddChoices(
                    "Submit Optimization Request",
                    "View Providers",
                    "Exit"));

        switch (choice)
        {
            case "Submit Optimization Request":
                await SubmitOptimizationRequest();
                break;
            case "View Providers":
                await GetProviders();
                break;
            case "Exit":
                AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
                return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
        AnsiConsole.Clear();
    }
}

async Task RunProviderMode()
{
    AnsiConsole.Write(new Rule("[blue]Provider Mode[/]").RuleStyle("blue"));
    AnsiConsole.WriteLine();
    
    AnsiConsole.MarkupLine("[yellow]Provider mode will be implemented in future sprints.[/]");
    AnsiConsole.MarkupLine("[grey]This will include:[/]");
    AnsiConsole.MarkupLine("  • Register provider capabilities");
    AnsiConsole.MarkupLine("  • Respond to estimate requests");
    AnsiConsole.MarkupLine("  • Accept/reject work assignments");
    AnsiConsole.WriteLine();
    
    AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
    Console.ReadKey(true);

    await Task.CompletedTask;
}

async Task SubmitOptimizationRequest()
{
    AnsiConsole.Write(new Rule("[yellow]Submit Optimization Request[/]"));
    AnsiConsole.WriteLine();

    // Generate random MotorRequest
    var random = new Random();
    var efficiencyClasses = new[] { MotorEfficiencyClass.IE1, MotorEfficiencyClass.IE2, MotorEfficiencyClass.IE3, MotorEfficiencyClass.IE4 };
    var startTime = DateTime.UtcNow.AddDays(random.Next(1, 100));

    var motorRequestDto = new OptimizationRequestDto
    {
        CustomerId = Guid.NewGuid().ToString(),
        MotorSpecs = new MotorSpecificationsDto
        {
            PowerKW = random.Next(50, 200),
            AxisHeightMM = random.Next(63, 315),
            CurrentEfficiency = efficiencyClasses[random.Next(efficiencyClasses.Length)].ToString(),
            TargetEfficiency = efficiencyClasses[random.Next(efficiencyClasses.Length)].ToString(),
            MalfunctionDescription = random.Next(0, 2) == 0 ? "Normal operation" : "Reduced efficiency, overheating"
        },
        Constraints = new OptimizationRequestConstraintsDto
        {
            MaxBudget = random.Next(0, 3) == 0 ? null : random.Next(5000, 20000),
            TimeWindow = new TimeWindowDto {
                StartTime = startTime,
                EndTime = startTime.AddHours(random.Next(100, 300))
            }
        }
    };

    // Display generated request
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Blue)
        .AddColumn("[yellow]Property[/]")
        .AddColumn("[yellow]Value[/]");

    table.AddRow("Customer ID", motorRequestDto.CustomerId);
    table.AddRow("Power", $"{motorRequestDto.MotorSpecs.PowerKW} kW");
    table.AddRow("Axis Height", $"{motorRequestDto.MotorSpecs.AxisHeightMM} mm");
    table.AddRow("Current Efficiency", motorRequestDto.MotorSpecs.CurrentEfficiency.ToString());
    table.AddRow("Target Efficiency", motorRequestDto.MotorSpecs.TargetEfficiency.ToString());
    table.AddRow("Malfunction", motorRequestDto.MotorSpecs.MalfunctionDescription ?? "-");
    table.AddRow("Max Budget", motorRequestDto.Constraints.MaxBudget.HasValue ? $"€{motorRequestDto.Constraints.MaxBudget.Value:N2}" : "No limit");
    table.AddRow("Time Window", $"{motorRequestDto.Constraints.TimeWindow.StartTime:yyyy-MM-dd HH:mm} to {motorRequestDto.Constraints.TimeWindow.EndTime:yyyy-MM-dd HH:mm}");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();

    if (!AnsiConsole.Confirm("[cyan]Submit this request?[/]"))
    {
        AnsiConsole.MarkupLine("[yellow]Request cancelled.[/]");
        return;
    }

    AnsiConsole.WriteLine();

    // Submit request to Gateway
    Guid requestId = Guid.Empty;
    List<OptimizationStrategyDto>? strategies = null;
    OptimizationPlanDto? plan = null;

    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("[yellow]Submitting request to Gateway...[/]", async ctx =>
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/optimization-requests", motorRequestDto);

                if (response.IsSuccessStatusCode)
                {
                    requestId = await response.Content.ReadFromJsonAsync<Guid>();
                    
                    ctx.Status("[green]✓ Request submitted successfully![/]");
                    AnsiConsole.MarkupLine($"[dim]Request ID: {requestId}[/]");
                    AnsiConsole.WriteLine();

                    // Poll for plan status
                    ctx.Status("[yellow]Waiting for optimization plan...[/]");
                    
                    // Initial delay to give Engine time to start processing
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    
                    var startTime = DateTime.UtcNow;
                    var timeout = TimeSpan.FromMinutes(10);
                    var pollInterval = TimeSpan.FromSeconds(2);
                    
                    while (DateTime.UtcNow - startTime < timeout)
                    {
                        await Task.Delay(pollInterval);
                        
                        try
                        {
                            var planResponse = await httpClient.GetAsync($"/api/optimization-requests/{requestId}/plan");
                            if (planResponse.IsSuccessStatusCode)
                            {
                                plan = await planResponse.Content.ReadFromJsonAsync<OptimizationPlanDto>();
                                if (plan != null)
                                {
                                    // Display status progress
                                    var statusMessage = plan.Status switch
                                    {
                                        "Draft" => "[cyan]Starting optimization...[/]",
                                        "MatchingProviders" => "[blue]Matching providers...[/]",
                                        "EstimatingCosts" => "[blue]Getting cost estimates...[/]",
                                        "GeneratingStrategies" => "[blue]Generating strategies...[/]",
                                        "AwaitingStrategySelection" => "[green]✓ Strategies ready![/]",
                                        "Failed" => "[red]✗ Optimization failed[/]",
                                        _ => $"[yellow]{plan.Status}...[/]"
                                    };
                                    
                                    ctx.Status($"{statusMessage} (elapsed: {(DateTime.UtcNow - startTime).TotalSeconds:F0}s)");
                                    
                                    if (plan.Status == "AwaitingStrategySelection")
                                    {
                                        // Strategies are in plan.Strategies
                                        strategies = plan.Strategies;
                                        break;
                                    }
                                    else if (plan.Status == "Failed")
                                    {
                                        AnsiConsole.MarkupLine("[red]✗ Optimization failed[/]");
                                        break;
                                    }
                                    // Continue polling for other statuses
                                }
                            }
                            else if (planResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                // Plan not created yet, continue polling
                                ctx.Status($"[yellow]Starting... (elapsed: {(DateTime.UtcNow - startTime).TotalSeconds:F0}s)[/]");
                                continue;
                            }
                            else
                            {
                                // Other error, log and continue
                                ctx.Status($"[yellow]Waiting (status {planResponse.StatusCode})...[/]");
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Continue polling on any error
                            ctx.Status($"[yellow]Retrying... ({ex.Message.Split('\n')[0]})[/]");
                            continue;
                        }
                    }
                    
                    if (plan == null)
                    {
                        AnsiConsole.MarkupLine("[red]✗ Timeout waiting for optimization plan[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Failed to submit: {response.StatusCode}[/]");
                    var error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[dim]{Markup.Escape(error)}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
                AnsiConsole.WriteException(ex);
            }
        });

    // Check if optimization failed
    if (plan != null && plan.Status == "Failed")
    {
        AnsiConsole.WriteLine();
        var errorPanel = new Panel($"[red]{Markup.Escape(plan.ErrorMessage ?? "Unknown error")}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Red)
            .Header("[red]Optimization Error[/]", Justify.Center);
        AnsiConsole.Write(errorPanel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Possible reasons:[/]");
        AnsiConsole.MarkupLine("[dim]- No providers available with required capabilities[/]");
        AnsiConsole.MarkupLine("[dim]- Providers did not respond within timeout[/]");
        AnsiConsole.MarkupLine("[dim]- No feasible solutions found for the given constraints[/]");
        return;
    }

    if (strategies == null || !strategies.Any())
    {
        AnsiConsole.MarkupLine("[yellow]No strategies available. Request may still be processing.[/]");
        return;
    }

    // Display strategies
    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[green]Available Optimization Strategies[/]").RuleStyle("green"));
    AnsiConsole.WriteLine();

    var strategiesTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Green)
        .AddColumn(new TableColumn("[yellow]#[/]").Centered())
        .AddColumn("[yellow]Strategy[/]")
        .AddColumn(new TableColumn("[yellow]Cost[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Duration[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Quality[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Emissions[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Warranty[/]").Centered())
        .AddColumn(new TableColumn("[yellow]Insurance[/]").Centered());

    int index = 1;
    foreach (var strategy in strategies)
    {
        strategiesTable.AddRow(
            index.ToString(),
            $"[bold]{strategy.StrategyName}[/]\n[dim]{strategy.Description}[/]",
            $"€{strategy.Metrics.TotalCost:N2}",
            $"{strategy.Metrics.TotalDuration.TotalDays:N1} days",
            strategy.Metrics.AverageQuality.ToString("N2"),
            $"{strategy.Metrics.TotalEmissionsKgCO2:N1} kg",
            strategy.Warranty?.Description ?? "-",
            strategy.Warranty?.IncludesInsurance == true ? "[green]✓[/]" : "[dim]-[/]"
        );
        index++;
    }

    AnsiConsole.Write(strategiesTable);
    AnsiConsole.WriteLine();

    // Customer selects strategy
    var selectedIndex = AnsiConsole.Prompt(
        new SelectionPrompt<int>()
            .Title("[cyan]Select your preferred strategy:[/]")
            .AddChoices(Enumerable.Range(1, strategies.Count).ToArray())
            .UseConverter(i => $"{i}. {strategies[i - 1].StrategyName}"));

    var selectedStrategy = strategies[selectedIndex - 1];

    // Send selection to Gateway and retrieve plan
    plan = null;
    
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("[yellow]Submitting strategy selection...[/]", async ctx =>
        {

            try
            {
                var response = await httpClient.PutAsJsonAsync($"/api/optimization-requests/{requestId}/strategy", selectedStrategy.Id);

                if (response.IsSuccessStatusCode)
                {
                    ctx.Status("[green]✓ Strategy selected![/]");
                    
                    // Wait and poll for plan to be generated
                    ctx.Status("[yellow]Waiting for optimization plan...[/]");
                    
                    var startTime = DateTime.UtcNow;
                    var timeout = TimeSpan.FromSeconds(30);
                    var pollInterval = TimeSpan.FromSeconds(1);
                    
                    while (DateTime.UtcNow - startTime < timeout)
                    {
                        await Task.Delay(pollInterval);
                        
                        try
                        {
                            var planResponse = await httpClient.GetAsync($"/api/optimization-requests/{requestId}/plan");
                            if (planResponse.IsSuccessStatusCode)
                            {
                                plan = await planResponse.Content.ReadFromJsonAsync<OptimizationPlanDto>();
                                
                                // Check if plan indicates an error
                                if (plan?.Status == "Failed")
                                {
                                    ctx.Status($"[red]✗ Optimization failed {plan.ErrorMessage} [/]");
                                    break;
                                }
                                
                                ctx.Status("[green]✓ Optimization plan retrieved![/]");
                                break;
                            }
                            if (planResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                // Plan not ready yet, continue polling
                                continue;
                            }
                        }
                        catch
                        {
                            // Continue polling
                        }
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Failed to select strategy: {response.StatusCode}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            }
        });
    
    // Display plan outside of Status context to avoid concurrent interactive operations
    AnsiConsole.WriteLine();
    
    if (plan != null)
    {
        DisplayOptimizationPlan(plan);
    }
    else
    {
        AnsiConsole.MarkupLine("[red]✗ Timeout waiting for optimization plan[/]");
        AnsiConsole.MarkupLine("[dim]The plan may still be processing. Try retrieving it later using the Request ID.[/]");
    }
}

void DisplayOptimizationPlan(OptimizationPlanDto plan)
{
    // Check if optimization failed
    if (plan.Status == "Failed")
    {
        AnsiConsole.Write(new Panel(new Markup("""
            [red]✗ Optimization Failed[/]
            
            The optimization process could not complete successfully.
            
            [yellow]Possible reasons:[/]
            • No providers available for required processes
            • Constraints cannot be satisfied
            • System error during optimization
            
            [dim]Please try again later or adjust your requirements.[/]
            """))
            .Header("[red]Optimization Error[/]")
            .BorderColor(Color.Red));
        return;
    }
    
    if (plan.SelectedStrategy == null)
    {
        AnsiConsole.MarkupLine("[red]✗ Plan has no selected strategy[/]");
        return;
    }

    AnsiConsole.Write(new Rule("[green]Final Optimization Plan[/]").RuleStyle("green"));
    AnsiConsole.WriteLine();
    
    // Plan overview
    var overviewTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Green)
        .AddColumn("[yellow]Property[/]")
        .AddColumn("[green]Value[/]");
    
    overviewTable.AddRow("Plan ID", $"[bold]{plan.Id}[/]");
    overviewTable.AddRow("Request ID", plan.RequestId.ToString());
    overviewTable.AddRow("Strategy", $"[bold]{plan.SelectedStrategy.StrategyName}[/]");
    overviewTable.AddRow("Priority", plan.SelectedStrategy.Priority.ToString());
    overviewTable.AddRow("Workflow Type", plan.SelectedStrategy.WorkflowType);
    overviewTable.AddRow("Status", $"[{GetStatusColor(plan.Status)}]{plan.Status.ToString()}[/]");
    overviewTable.AddRow("Total Cost", $"€{plan.SelectedStrategy.Metrics.TotalCost:N2}");
    overviewTable.AddRow("Total Duration", $"{plan.SelectedStrategy.Metrics.TotalDuration.TotalDays:N1} days ({plan.SelectedStrategy.Metrics.TotalDuration.TotalHours:N1} hours)");
    overviewTable.AddRow("Average Quality", $"{plan.SelectedStrategy.Metrics.AverageQuality:P0}");
    overviewTable.AddRow("Total Emissions", $"{plan.SelectedStrategy.Metrics.TotalEmissionsKgCO2:N2} kg CO₂");
    overviewTable.AddRow("Warranty", plan.SelectedStrategy.Warranty?.Description ?? "-");
    overviewTable.AddRow("Insurance", plan.SelectedStrategy.Warranty?.IncludesInsurance == true ? "[green]✓ Included[/]" : "[dim]Not included[/]");
    overviewTable.AddRow("Solver Status", plan.SelectedStrategy.Metrics.SolverStatus ?? "-");
    overviewTable.AddRow("Objective Value", plan.SelectedStrategy.Metrics.ObjectiveValue.ToString("N6"));
    overviewTable.AddRow("Created At", plan.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));
    
    AnsiConsole.Write(new Panel(overviewTable)
        .Header("[yellow]Plan Overview[/]")
        .BorderColor(Color.Green));
    
    AnsiConsole.WriteLine();
    
    // Process steps with providers
    if (plan.SelectedStrategy.Steps?.Any() == true)
    {
        var stepsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[yellow]Step[/]").Centered())
            .AddColumn("[yellow]Process[/]")
            .AddColumn("[yellow]Provider[/]")
            .AddColumn(new TableColumn("[yellow]Provider ID[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Cost[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Quality[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]CO₂[/]").RightAligned());
        
        foreach (var step in plan.SelectedStrategy.Steps.OrderBy(s => s.StepNumber))
        {
            stepsTable.AddRow(
                step.StepNumber.ToString(),
                step.Process.ToString(),
                $"[bold]{step.SelectedProviderName}[/]",
                step.SelectedProviderId.ToString()[..8] + "...",
                $"€{step.Estimate.Cost:N2}",
                $"{step.Estimate.QualityScore:P0}",
                $"{step.Estimate.EmissionsKgCO2:N2} kg"
            );
        }
        
        AnsiConsole.Write(new Panel(stepsTable)
            .Header($"[blue]Execution Plan ({plan.SelectedStrategy.Steps.Count} steps)[/]")
            .BorderColor(Color.Blue));
        
        AnsiConsole.WriteLine();
        
        // Display Timeline if available
        DisplayTimeline(plan.SelectedStrategy.Steps);
    }
    
    // Success summary
    AnsiConsole.Write(new Panel($"""
        [green]✓ Your optimization plan is ready for execution![/]
        
        [bold]Plan ID:[/] [cyan]{plan.Id}[/]
        [bold]Strategy:[/] {plan.SelectedStrategy.StrategyName}
        
        [dim]{plan.SelectedStrategy.Description}[/]
        
        [yellow]Next steps:[/]
        • Providers will be notified to prepare for execution
        • You will receive updates as work progresses
        • Track progress via Plan ID: {plan.Id}
        """)
        .Header("[green]Plan Confirmed[/]")
        .BorderColor(Color.Green)
        .Padding(1, 1));
    
    AnsiConsole.WriteLine();
    
    // Display full plan as JSON
    AnsiConsole.WriteLine();
    var json = JsonSerializer.Serialize(plan, new JsonSerializerOptions 
    { 
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    
    //AnsiConsole.Write(new Panel(Markup.Escape(json))
    //    .Header("[yellow]Complete Optimization Plan (JSON)[/]")
    //    .BorderColor(Color.Yellow)
    //    .Expand());
}

string GetStatusColor(string status)
{
    return status switch
    {
        "InProgress" => "blue",
        "Completed" => "green",
        "Failed" => "red",
        _ => "white"
    };
}

async Task GetProviders()
{
    await AnsiConsole.Status()
        .StartAsync("Getting providers list...", async ctx =>
        {
            try
            {
                var response = await httpClient.GetAsync("/api/providers");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<ProviderDto>>();
                    
                    if (result != null)
                    {
                        var table = new Table()
                            .Border(TableBorder.Rounded)
                            .BorderColor(Color.Green)
                            .AddColumn("[yellow]Provider ID[/]")
                            .AddColumn("[yellow]Type[/]")
                            .AddColumn("[yellow]Name[/]")
                            .AddColumn("[yellow]Status[/]");
                        
                        foreach (var provider in result)
                        {
                            table.AddRow(
                                provider.Id.ToString(),
                                provider.Type,
                                provider.Name,
                                provider.Enabled ? "[green]Active[/]" : "[dim]Inactive[/]"
                            );
                        }
                        
                        AnsiConsole.Write(table);
                        AnsiConsole.MarkupLine($"\n[green]Total Providers:[/] {result.Count}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No providers registered yet[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Error: {response.StatusCode}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
            }
        });
}

void DisplayTimeline(List<ProcessStepDto> steps)
{
    // Check if any steps have timeline data
    if (!steps.Any(s => s.AllocatedSlot != null))
    {
        AnsiConsole.MarkupLine("[dim]No timeline data available (time window was not specified)[/]");
        return;
    }
    
    AnsiConsole.Write(new Rule("[cyan]Execution Timeline[/]").RuleStyle("cyan"));
    AnsiConsole.WriteLine();
    
    var orderedSteps = steps.OrderBy(s => s.StepNumber).ToList();
    
    // Timeline table
    var timelineTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Aqua)
        .AddColumn(new TableColumn("[yellow]Step[/]").Centered())
        .AddColumn("[yellow]Process[/]")
        .AddColumn("[yellow]Provider[/]")
        .AddColumn("[yellow]Scheduled Time[/]")
        .AddColumn(new TableColumn("[yellow]Duration[/]").RightAligned());
    
    foreach (var step in orderedSteps)
    {
        var slotStr = step.AllocatedSlot != null
            ? $"{step.AllocatedSlot.Start:yyyy-MM-dd HH:mm} - {step.AllocatedSlot.End:HH:mm}"
            : "[dim]Not scheduled[/]";
            
        var duration = step.AllocatedSlot != null
            ? $"{(step.AllocatedSlot.End - step.AllocatedSlot.Start).TotalHours:N1}h"
            : "-";
        
        timelineTable.AddRow(
            step.StepNumber.ToString(),
            $"[bold]{step.Process}[/]",
            step.SelectedProviderName,
            slotStr,
            duration
        );
    }
    
    AnsiConsole.Write(new Panel(timelineTable)
        .Header("[cyan]Process Schedule[/]")
        .BorderColor(Color.Aqua));

    
    AnsiConsole.WriteLine();
    
    // Gantt-style visualization
    if (orderedSteps.All(s => s.AllocatedSlot != null))
    {
        DisplayGanttChart(orderedSteps);
    }
    
    // Display available time slots for each step
    DisplayAvailableTimeSlots(orderedSteps);
}

void DisplayGanttChart(List<ProcessStepDto> steps)
{
    var allSlots = steps.Where(s => s.AllocatedSlot != null).Select(s => s.AllocatedSlot!).ToList();
    var minStart = allSlots.Min(s => s.StartTime);
    var maxEnd = allSlots.Max(s => s.EndTime);
    var totalDuration = maxEnd - minStart;
    
    AnsiConsole.Write(new Rule("[green]Gantt Chart[/]").RuleStyle("green"));
    AnsiConsole.WriteLine();
    
    var ganttTable = new Table()
        .Border(TableBorder.None)
        .HideHeaders()
        .AddColumn(new TableColumn("").Width(20))
        .AddColumn(new TableColumn("").Width(60));
    
    const int chartWidth = 50;
    
    foreach (var step in steps.OrderBy(s => s.StepNumber))
    {
        if (step.AllocatedSlot == null) continue;
        
        var slot = step.AllocatedSlot;
        var label = $"{step.Process} ({step.StepNumber})";
        
        // Build bar with segments (work = green, break = red)
        var bar = "";
        if (slot.Segments != null && slot.Segments.Any())
        {
            foreach (var segment in slot.Segments.OrderBy(s => s.SegmentOrder))
            {
                var segmentStartOffset = (segment.StartTime - minStart).TotalHours / totalDuration.TotalHours;
                var segmentDuration = (segment.EndTime - segment.StartTime).TotalHours / totalDuration.TotalHours;
                
                var segmentStartPos = (int)(segmentStartOffset * chartWidth);
                var segmentLength = Math.Max(1, (int)(segmentDuration * chartWidth));
                
                // Pad to start position (only if not first segment of this step)
                if (bar.Length < segmentStartPos)
                {
                    bar += new string(' ', segmentStartPos - bar.Length);
                }
                
                var isWork = segment.SegmentType == "WorkingTime";
                var color = isWork ? "green" : "red";
                var barChars = new string('█', segmentLength);
                bar += $"[{color}]{barChars}[/]";
            }
        }
        else
        {
            // Fallback: no segments, show entire slot as one bar
            var startOffset = (slot.StartTime - minStart).TotalHours / totalDuration.TotalHours;
            var duration = (slot.EndTime - slot.StartTime).TotalHours / totalDuration.TotalHours;
            
            var startPos = (int)(startOffset * chartWidth);
            var barLength = Math.Max(1, (int)(duration * chartWidth));
            
            var barChars = new string(' ', startPos) + new string('█', barLength);
            bar = $"[cyan]{barChars.PadRight(chartWidth)}[/]";
        }
        
        ganttTable.AddRow(
            label.Length > 18 ? label[..18] + ".." : label,
            bar
        );
    }
    
    AnsiConsole.Write(new Panel(ganttTable)
        .Header($"[green]Timeline: {minStart:MMM dd HH:mm} → {maxEnd:MMM dd HH:mm} ({totalDuration.TotalDays:N1} days) | [green]█[/] Work [yellow]█[/] Break[/]")
        .BorderColor(Color.Green));
    
    AnsiConsole.WriteLine();
}

void DisplayAvailableTimeSlots(List<ProcessStepDto> steps)
{
    // Check if any steps have available time slots
    if (!steps.Any(s => s.Estimate.AvailableTimeSlots?.Any() == true))
    {
        return;
    }
    
    AnsiConsole.Write(new Rule("[yellow]Available Time Slots[/]").RuleStyle("yellow"));
    AnsiConsole.WriteLine();
    
    foreach (var step in steps.OrderBy(s => s.StepNumber))
    {
        if (step.Estimate.AvailableTimeSlots?.Any() != true)
            continue;
        
        var slotsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .AddColumn(new TableColumn("[yellow]Slot #[/]").Centered())
            .AddColumn("[yellow]Start Time[/]")
            .AddColumn("[yellow]End Time[/]")
            .AddColumn(new TableColumn("[yellow]Duration[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Selected[/]").Centered());
        
        int slotNum = 1;
        foreach (var slot in step.Estimate.AvailableTimeSlots)
        {
            var isSelected = step.AllocatedSlot != null &&
                           slot.StartTime >= step.AllocatedSlot.Start &&
                           slot.EndTime <= step.AllocatedSlot.End;
            
            var duration = slot.EndTime - slot.StartTime;
            
            slotsTable.AddRow(
                slotNum.ToString(),
                slot.StartTime.ToString("yyyy-MM-dd HH:mm"),
                slot.EndTime.ToString("yyyy-MM-dd HH:mm"),
                $"{duration.TotalHours:N1}h",
                isSelected ? "[green]✓[/]" : ""
            );
            slotNum++;
        }
        
        AnsiConsole.Write(new Panel(slotsTable)
            .Header($"[yellow]Step {step.StepNumber}: {step.Process} - {step.SelectedProviderName}[/]")
            .BorderColor(Color.Yellow));
        
        AnsiConsole.WriteLine();
    }
}