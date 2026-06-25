using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class ScenarioData
{
    [JsonProperty("schema_version")]
    public string SchemaVersion { get; set; }

    [JsonProperty("scenario_meta")]
    public ScenarioMeta ScenarioMeta { get; set; }

    [JsonProperty("initial_state")]
    public ScenarioInitialState InitialState { get; set; }

    [JsonProperty("hotspots")]
    public List<ScenarioHotspot> Hotspots { get; set; }

    [JsonProperty("ehr_config")]
    public ScenarioEhrConfig EhrConfig { get; set; }

    [JsonProperty("rules")]
    public ScenarioRules Rules { get; set; }

    [JsonProperty("nodes")]
    public List<ScenarioNode> Nodes { get; set; }

    [JsonProperty("logging")]
    public ScenarioLogging Logging { get; set; }
}

[Serializable]
public class ScenarioMeta
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("estimated_duration_minutes")]
    public int EstimatedDurationMinutes { get; set; }

    [JsonProperty("difficulty")]
    public string Difficulty { get; set; }

    [JsonProperty("learning_goals")]
    public List<string> LearningGoals { get; set; }
}

[Serializable]
public class ScenarioInitialState
{
    [JsonProperty("time_elapsed")]
    public int TimeElapsed { get; set; }

    [JsonProperty("current_score")]
    public int CurrentScore { get; set; }

    [JsonProperty("flags")]
    public Dictionary<string, bool> Flags { get; set; }

    [JsonProperty("vitals")]
    public ScenarioVitalsSnapshot Vitals { get; set; }

    [JsonProperty("ui")]
    public ScenarioUIState UI { get; set; }
}

[Serializable]
public class ScenarioUIState
{
    [JsonProperty("active_hotspots")]
    public List<string> ActiveHotspots { get; set; }

    [JsonProperty("monitor_alert")]
    public bool MonitorAlert { get; set; }
}

[Serializable]
public class ScenarioHotspot
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }
}

[Serializable]
public class ScenarioEhrConfig
{
    [JsonProperty("forms")]
    public Dictionary<string, ScenarioEhrForm> Forms { get; set; }
}

[Serializable]
public class ScenarioEhrForm
{
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("fields")]
    public List<string> Fields { get; set; }
}

[Serializable]
public class ScenarioRules
{
    [JsonProperty("global_rules")]
    public List<ScenarioGlobalRule> GlobalRules { get; set; }
}

[Serializable]
public class ScenarioGlobalRule
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("condition")]
    public Dictionary<string, ScenarioComparison> Condition { get; set; }

    [JsonProperty("effects")]
    public List<ScenarioRuleEffect> Effects { get; set; }
}

[Serializable]
public class ScenarioComparison
{
    [JsonProperty("lt")]
    public double? LessThan { get; set; }

    [JsonProperty("lte")]
    public double? LessThanOrEqual { get; set; }

    [JsonProperty("gt")]
    public double? GreaterThan { get; set; }

    [JsonProperty("gte")]
    public double? GreaterThanOrEqual { get; set; }

    [JsonProperty("eq")]
    public double? Equal { get; set; }
}

[Serializable]
public class ScenarioRuleEffect
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("target")]
    public string Target { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("style")]
    public string Style { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}

[Serializable]
public class ScenarioNode
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("next_node_id")]
    public string NextNodeId { get; set; }

    [JsonProperty("options")]
    public List<ScenarioOption> Options { get; set; }

    [JsonProperty("timeout")]
    public ScenarioTimeout Timeout { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("gate_requirements")]
    public ScenarioGateRequirements GateRequirements { get; set; }

    [JsonProperty("feedback_blocked")]
    public string FeedbackBlocked { get; set; }

    [JsonProperty("feedback_success")]
    public string FeedbackSuccess { get; set; }

    [JsonProperty("effects_on_pass")]
    public ScenarioEffects EffectsOnPass { get; set; }

    [JsonProperty("debrief_config")]
    public ScenarioDebriefConfig DebriefConfig { get; set; }
}

[Serializable]
public class ScenarioOption
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("target_hotspot")]
    public string TargetHotspot { get; set; }

    [JsonProperty("effects")]
    public ScenarioEffects Effects { get; set; }

    [JsonProperty("next_node_id")]
    public string NextNodeId { get; set; }
}

[Serializable]
public class ScenarioEffects
{
    [JsonProperty("score_delta")]
    public int? ScoreDelta { get; set; }

    [JsonProperty("state_update")]
    public JObject StateUpdate { get; set; }

    [JsonProperty("vitals_update")]
    public ScenarioVitalsUpdate VitalsUpdate { get; set; }

    [JsonProperty("toast")]
    public string Toast { get; set; }

    [JsonProperty("additional_effects")]
    public List<ScenarioRuleEffect> AdditionalEffects { get; set; }
}

[Serializable]
public class ScenarioTimeout
{
    [JsonProperty("seconds")]
    public int Seconds { get; set; }

    [JsonProperty("on_timeout_effects")]
    public ScenarioEffects OnTimeoutEffects { get; set; }

    [JsonProperty("next_node_id")]
    public string NextNodeId { get; set; }
}

[Serializable]
public class ScenarioGateRequirements
{
    [JsonProperty("target_hotspot")]
    public string TargetHotspot { get; set; }

    [JsonProperty("required_forms")]
    public List<ScenarioGateFormRequirement> RequiredForms { get; set; }
}

[Serializable]
public class ScenarioGateFormRequirement
{
    [JsonProperty("form_id")]
    public string FormId { get; set; }

    [JsonProperty("fields")]
    public List<string> Fields { get; set; }
}

[Serializable]
public class ScenarioDebriefConfig
{
    [JsonProperty("show_score")]
    public bool ShowScore { get; set; }

    [JsonProperty("show_decision_path")]
    public bool ShowDecisionPath { get; set; }

    [JsonProperty("highlight_missed_docs")]
    public bool HighlightMissedDocs { get; set; }

    [JsonProperty("export_log")]
    public bool ExportLog { get; set; }
}

[Serializable]
public class ScenarioLogging
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("log_events")]
    public List<string> LogEvents { get; set; }

    [JsonProperty("export_format")]
    public string ExportFormat { get; set; }
}

[Serializable]
public class ScenarioVitalsSnapshot
{
    [JsonProperty("hr")]
    public int HeartRate { get; set; }

    [JsonProperty("spo2")]
    public int OxygenSaturation { get; set; }

    [JsonProperty("rr")]
    public int RespiratoryRate { get; set; }

    [JsonProperty("bp")]
    public string BloodPressure { get; set; }

    [JsonProperty("temp")]
    public float Temperature { get; set; }
}

[Serializable]
public class ScenarioVitalsUpdate
{
    [JsonProperty("hr")]
    public int? HeartRate { get; set; }

    [JsonProperty("spo2")]
    public int? OxygenSaturation { get; set; }

    [JsonProperty("rr")]
    public int? RespiratoryRate { get; set; }

    [JsonProperty("bp")]
    public string BloodPressure { get; set; }

    [JsonProperty("temp")]
    public float? Temperature { get; set; }
}
