import { Workflow } from '../App'

interface WorkflowListProps {
  workflows: Workflow[]
  onEdit: (workflow: Workflow) => void
  onDelete: (id: string) => void
  onExecute?: (id: string) => void
  onToggleStatus?: (id: string, currentStatus: Workflow['status']) => void
  onDesign?: (id: string) => void
  executingWorkflows?: Set<string>
}

const WorkflowList = ({ workflows, onEdit, onDelete, onExecute, onToggleStatus, onDesign, executingWorkflows = new Set() }: WorkflowListProps) => {
  const getStatusColor = (status: Workflow['status']) => {
    switch (status) {
      case 'active':
        return 'bg-emerald-100 text-emerald-800 border-emerald-200'
      case 'inactive':
        return 'bg-red-100 text-red-800 border-red-200'
      case 'draft':
        return 'bg-amber-100 text-amber-800 border-amber-200'
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200'
    }
  }

  if (workflows.length === 0) {
    return (
      <div className="card text-center py-16">
        <div className="mx-auto w-24 h-24 bg-gradient-to-r from-blue-100 to-purple-100 rounded-full flex items-center justify-center mb-6">
          <svg className="w-12 h-12 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h3 className="text-xl font-semibold text-gray-900 mb-3">No workflows yet</h3>
        <p className="text-gray-600 mb-6 max-w-md mx-auto">Get started by creating your first workflow to automate your processes and boost productivity.</p>
        <button
          onClick={() => {
            // Trigger the add workflow functionality by finding and clicking the hidden button
            const addButton = document.querySelector('[data-add-workflow]') as HTMLButtonElement;
            if (addButton) addButton.click();
          }}
          className="btn-primary inline-flex items-center gap-2 animate-pulse hover:animate-none"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          Create Your First Workflow
        </button>
      </div>
    )
  }

  return (
    <div className="card overflow-hidden">
      <div className="card-header bg-gradient-to-r from-gray-50 to-white">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold text-gray-900">Workflows</h2>
          <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
            {workflows.length} {workflows.length === 1 ? 'workflow' : 'workflows'}
          </span>
        </div>
      </div>
      <div className="divide-y divide-gray-100">
        {workflows.map((workflow) => (
          <div key={workflow.id} className="group px-4 sm:px-6 py-4 sm:py-5 hover:bg-gradient-to-r hover:from-blue-50 hover:to-transparent transition-all duration-200">
            <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
              <div className="flex-1 min-w-0">
                <div className="flex flex-col sm:flex-row sm:items-center gap-2 sm:gap-3 mb-2">
                  <h3 className="text-base font-semibold text-gray-900 group-hover:text-blue-900 transition-colors">
                    {workflow.name}
                  </h3>
                  <span className={`inline-flex items-center px-2 sm:px-3 py-1 rounded-full text-xs font-medium border ${getStatusColor(workflow.status)} self-start`}>
                    {workflow.status.charAt(0).toUpperCase() + workflow.status.slice(1)}
                  </span>
                </div>
                {workflow.description && (
                  <p className="text-sm text-gray-600 mb-3 line-clamp-2">{workflow.description}</p>
                )}
                <div className="flex flex-col sm:flex-row sm:items-center gap-2 sm:gap-4 text-xs text-gray-500">
                  <span className="flex items-center gap-1">
                    <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    Created {workflow.createdAt.toLocaleDateString()}
                  </span>
                  <span className="flex items-center gap-1">
                    <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    Updated {workflow.updatedAt.toLocaleDateString()}
                  </span>
                </div>
              </div>
              <div className="flex items-center gap-2 self-end sm:self-start sm:ml-4">
                {onDesign && (
                  <button
                    onClick={() => onDesign(workflow.id)}
                    className="inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-purple-700 bg-purple-50 hover:bg-purple-100 rounded-lg transition-colors duration-200"
                    title="Design workflow in n8n"
                  >
                    <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    <span className="hidden sm:inline">Design</span>
                    <span className="sm:hidden">Design</span>
                  </button>
                )}
                {onExecute && (
                  <button
                    onClick={() => onExecute(workflow.id)}
                    disabled={executingWorkflows.has(workflow.id)}
                    className="inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-green-700 bg-green-50 hover:bg-green-100 disabled:opacity-50 disabled:cursor-not-allowed rounded-lg transition-colors duration-200"
                  >
                    {executingWorkflows.has(workflow.id) ? (
                      <svg className="w-3 h-3 animate-spin" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                    ) : (
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                      </svg>
                    )}
                    <span className="hidden sm:inline">
                      {executingWorkflows.has(workflow.id) ? 'Running...' : 'Execute'}
                    </span>
                    <span className="sm:hidden">
                      {executingWorkflows.has(workflow.id) ? '...' : 'Run'}
                    </span>
                  </button>
                )}
                {onToggleStatus && (
                  <button
                    onClick={() => onToggleStatus(workflow.id, workflow.status)}
                    className={`inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium rounded-lg transition-colors duration-200 ${
                      workflow.status === 'active'
                        ? 'text-orange-700 bg-orange-50 hover:bg-orange-100'
                        : 'text-green-700 bg-green-50 hover:bg-green-100'
                    }`}
                  >
                    <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      {workflow.status === 'active' ? (
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 9v6m0 0l-3-3m3 3l3-3" />
                      ) : (
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.828 14.828a4 4 0 01-5.656 0M9 10h1m4 0h1m-6 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      )}
                    </svg>
                    <span className="hidden sm:inline">
                      {workflow.status === 'active' ? 'Deactivate' : 'Activate'}
                    </span>
                    <span className="sm:hidden">
                      {workflow.status === 'active' ? 'Stop' : 'Start'}
                    </span>
                  </button>
                )}
                <button
                  onClick={() => onEdit(workflow)}
                  className="inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-blue-700 bg-blue-50 hover:bg-blue-100 rounded-lg transition-colors duration-200"
                >
                  <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                  <span className="hidden sm:inline">Edit</span>
                  <span className="sm:hidden">Edit</span>
                </button>
                <button
                  onClick={() => onDelete(workflow.id)}
                  className="inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-red-700 bg-red-50 hover:bg-red-100 rounded-lg transition-colors duration-200"
                >
                  <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                  <span className="hidden sm:inline">Delete</span>
                  <span className="sm:hidden">Del</span>
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

export default WorkflowList