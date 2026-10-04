import { useDeferredValue, useState } from 'react';
import { Avatar, Button, DataList, Input, Snackbar, getApiErrorMessage } from '@blanko/shared';
import type { ClientDto, CreateClientRequest } from '@blanko/shared';
import { useCreateClient, useClients, useDeleteClient, useUpdateClient } from '../../../entities/clients/api/clientQueries';
import type { ClientFormValues } from '../model/clientSchema';
import { ClientForm } from './ClientForm';

export function ClientsPage() {
  const [search, setSearch] = useState('');
  const [selectedClient, setSelectedClient] = useState<ClientDto | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [notice, setNotice] = useState<{ message: string; variant: 'success' | 'error' } | null>(null);
  const clientsQuery = useClients(useDeferredValue(search));
  const createClient = useCreateClient();
  const updateClient = useUpdateClient();
  const deleteClient = useDeleteClient();
  const isSaving = createClient.isPending || updateClient.isPending || deleteClient.isPending;

  const closeForm = () => {
    setIsFormOpen(false);
    setSelectedClient(null);
  };

  const saveClient = async (values: ClientFormValues) => {
    setNotice(null);
    const request: CreateClientRequest = {
      name: values.name,
      phone: values.phone || null,
      contacts: values.contacts,
      birthDate: values.birthDate ? `${values.birthDate}T00:00:00Z` : null,
      notes: values.notes || null,
    };

    try {
      if (selectedClient) await updateClient.mutateAsync({ id: selectedClient.id, request });
      else await createClient.mutateAsync(request);
      setNotice({ message: selectedClient ? 'Данные клиента обновлены' : 'Клиент добавлен', variant: 'success' });
      closeForm();
    } catch (error: unknown) {
      setNotice({ message: getApiErrorMessage(error), variant: 'error' });
    }
  };

  const removeClient = async () => {
    if (!selectedClient) return;
    setNotice(null);
    try {
      await deleteClient.mutateAsync(selectedClient.id);
      setNotice({ message: 'Клиент удалён', variant: 'success' });
      closeForm();
    } catch (error: unknown) {
      setNotice({ message: getApiErrorMessage(error), variant: 'error' });
    }
  };

  return (
    <section className="crm-page">
      <div className="crm-page-toolbar">
        <Input
          aria-label="Поиск клиентов"
          placeholder="Поиск по имени или телефону"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <Button onClick={() => { setSelectedClient(null); setIsFormOpen(true); }}>
          Добавить клиента
        </Button>
      </div>
      {notice && <Snackbar message={notice.message} variant={notice.variant} />}
      {clientsQuery.isError && <Snackbar message={getApiErrorMessage(clientsQuery.error)} />}
      <DataList<ClientDto>
        items={clientsQuery.data ?? []}
        getId={(client) => client.id}
        loading={clientsQuery.isLoading}
        emptyState="Клиентов пока нет"
        onItemClick={(client) => { setSelectedClient(client); setIsFormOpen(true); }}
        renderCardLeading={(client) => <Avatar name={client.name} />}
        renderCardTitle={(client) => client.name}
        renderCardSubtitle={(client) => client.phone || 'Без телефона'}
        renderCardDescription={(client) => client.contacts[0]?.value ?? ''}
        columns={[
          { key: 'name', label: 'Имя', render: (client) => client.name },
          { key: 'phone', label: 'Телефон', render: (client) => client.phone ?? '—' },
          { key: 'contact', label: 'Контакт', render: (client) => client.contacts[0]?.value ?? '—' },
        ]}
      />
      {isFormOpen && (
        <ClientForm
          key={selectedClient?.id ?? 'new'}
          client={selectedClient}
          loading={isSaving}
          onClose={closeForm}
          onSubmit={saveClient}
          onDelete={selectedClient ? removeClient : undefined}
        />
      )}
    </section>
  );
}
