import { useState } from 'react';
import { Button, DataList, Snackbar, getApiErrorMessage } from '@blanko/shared';
import type { ServiceDto } from '@blanko/shared';
import { useCreateService, useDeleteService, useServices, useUpdateService } from '../../../entities/services/api/serviceQueries';
import type { ServiceFormValues } from '../model/serviceSchema';
import { ServiceForm } from './ServiceForm';

export function ServicesPage() {
  const [selectedService, setSelectedService] = useState<ServiceDto | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [notice, setNotice] = useState<{ message: string; variant: 'success' | 'error' } | null>(null);
  const servicesQuery = useServices();
  const createService = useCreateService();
  const updateService = useUpdateService();
  const deleteService = useDeleteService();
  const isSaving = createService.isPending || updateService.isPending || deleteService.isPending;

  const closeForm = () => {
    setIsFormOpen(false);
    setSelectedService(null);
  };

  const saveService = async (values: ServiceFormValues) => {
    setNotice(null);
    const request = { ...values, description: values.description || null };
    try {
      if (selectedService) await updateService.mutateAsync({ id: selectedService.id, request });
      else await createService.mutateAsync(request);
      setNotice({ message: selectedService ? 'Данные услуги обновлены' : 'Услуга добавлена', variant: 'success' });
      closeForm();
    } catch (error: unknown) {
      setNotice({ message: getApiErrorMessage(error), variant: 'error' });
    }
  };

  const removeService = async () => {
    if (!selectedService) return;
    setNotice(null);
    try {
      await deleteService.mutateAsync(selectedService.id);
      setNotice({ message: 'Услуга удалена', variant: 'success' });
      closeForm();
    } catch (error: unknown) {
      setNotice({ message: getApiErrorMessage(error), variant: 'error' });
    }
  };

  return (
    <section className="crm-page">
      <div className="crm-page-toolbar">
        <h2>Прайс-лист</h2>
        <Button onClick={() => { setSelectedService(null); setIsFormOpen(true); }}>
          Добавить услугу
        </Button>
      </div>
      {notice && <Snackbar message={notice.message} variant={notice.variant} />}
      {servicesQuery.isError && <Snackbar message={getApiErrorMessage(servicesQuery.error)} />}
      <DataList<ServiceDto>
        items={servicesQuery.data ?? []}
        getId={(service) => service.id}
        loading={servicesQuery.isLoading}
        emptyState="Услуг пока нет"
        onItemClick={(service) => { setSelectedService(service); setIsFormOpen(true); }}
        renderCardLeading={(service) => (
          <span className="crm-avatar crm-avatar--service" aria-hidden="true">
            {service.name.trim().charAt(0).toLocaleUpperCase() || '✦'}
          </span>
        )}
        renderCardTitle={(service) => service.name}
        renderCardSubtitle={(service) => `${service.durationMinutes} мин · ${service.price.toLocaleString('ru-RU')} ₽`}
        renderCardDescription={(service) => service.description ?? ''}
        columns={[
          { key: 'name', label: 'Наименование', render: (service) => service.name },
          { key: 'duration', label: 'Длительность', render: (service) => `${service.durationMinutes} мин` },
          { key: 'price', label: 'Цена', render: (service) => `${service.price.toLocaleString('ru-RU')} ₽` },
          { key: 'description', label: 'Описание', render: (service) => service.description ?? '—' },
        ]}
      />
      {isFormOpen && (
        <ServiceForm
          key={selectedService?.id ?? 'new'}
          service={selectedService}
          loading={isSaving}
          onClose={closeForm}
          onSubmit={saveService}
          onDelete={selectedService ? removeService : undefined}
        />
      )}
    </section>
  );
}
